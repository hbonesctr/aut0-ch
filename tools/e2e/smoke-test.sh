#!/usr/bin/env bash
# AUT0-CH end-to-end smoke test.
#
# Starts the full stack from docs/scripts/docker-compose.yml (AUT0-CH images must already be built,
# e.g. with tools/build-images.sh), configures Keycloak, logs in through the API, uploads a STIG
# Viewer 3 CKLB, and checks that it was stored, scored, and can be downloaded again as a CKL.
#
#   sudo tools/e2e/smoke-test.sh            # run, then stop the stack
#   KEEP=1 sudo tools/e2e/smoke-test.sh     # leave the stack running afterwards
#
# Needs: docker with compose plugin, curl, python3. Uses ports 8080 on this host.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
STACK="$ROOT/docs/scripts"
FIXTURE="$ROOT/tools/e2e/fixtures/rhel9-v2r10-blank.cklb"
WORK="$(mktemp -d)"
IP="${HOST_IP:-$(hostname -I | awk '{print $1}')}"
BASE="http://$IP:8080"
CI_USER="ci-admin"
CI_PASS='CiTest##2026Aut0!!'   # meets the realm policy: 2 special, 2 upper, 2 digits, 12+ chars
export TERM="${TERM:-dumb}"

step() { echo; echo "==> $*"; }
fail() { echo "FAIL: $*" >&2; exit 1; }

cleanup() {
  local rc=$?
  if [ $rc -ne 0 ]; then
    step "Test failed - container status and recent logs"
    (cd "$STACK" && docker compose ps) || true
    for c in openrmf-read-api openrmf-web openrmf-keycloak openrmf-score-nats-message-client openrmf-report-nats-message-client openrmf-system-nats-message-client; do
      echo "----- $c -----"; docker logs --tail 60 "$c" 2>&1 || true
    done
  fi
  if [ -z "${KEEP:-}" ]; then (cd "$STACK" && docker compose down -v >/dev/null 2>&1) || true; fi
  rm -rf "$WORK"
  exit $rc
}
trap cleanup EXIT

wait_for() { # url, seconds, description
  local url=$1 secs=$2 what=$3 i
  for ((i = 0; i < secs; i += 5)); do
    if curl -fsS -o /dev/null "$url" 2>/dev/null; then echo "$what is up (${i}s)"; return 0; fi
    sleep 5
  done
  fail "$what did not come up within ${secs}s ($url)"
}

step "Configure the stack for host $IP"
cd "$STACK"
sed -i "s#^JWTAUTHORITY=.*#JWTAUTHORITY=$BASE/auth/realms/openrmf#" .env
sed -i "s#^GF_SERVER_DOMAIN=.*#GF_SERVER_DOMAIN=$IP#" .grafana
cat .env

step "Start the stack"
COMPOSE_PARALLEL_LIMIT=30 docker compose up -d --quiet-pull
wait_for "$BASE/" 300 "Web UI"
wait_for "$BASE/auth/realms/master" 420 "Keycloak"

step "Create the openrmf realm, roles, client and first user (docs/scripts/keycloak/setup-realm-linux.sh)"
printf '%s\n%s\n' "$IP" "$CI_USER" | bash keycloak/setup-realm-linux.sh > "$WORK/realm.log" 2>&1 || { cat "$WORK/realm.log"; fail "realm setup script failed"; }
grep -qi "error" "$WORK/realm.log" && { echo "(realm script printed errors - continuing, checked below)"; grep -i "error" "$WORK/realm.log" | head -5; }

KC() { docker exec -i openrmf-keycloak /opt/keycloak/bin/kcadm.sh "$@"; }
KC config credentials --server http://localhost:8080/auth --realm master --user admin --password admin >/dev/null
CID=$(KC get clients -r openrmf -q clientId=openrmf --fields id --format csv --noquotes | tail -1)
[ -n "$CID" ] || fail "openrmf client was not created"
KC update "clients/$CID" -r openrmf -s directAccessGrantsEnabled=true      # allow API login for this test
UID_=$(KC get users -r openrmf -q username=$CI_USER --fields id --format csv --noquotes | tail -1)
[ -n "$UID_" ] || fail "user $CI_USER was not created"
KC update "users/$UID_" -r openrmf -s 'requiredActions=[]' -s email=ci@example.invalid -s firstName=CI -s lastName=Admin -s emailVerified=true
KC set-password -r openrmf --username "$CI_USER" --new-password "$CI_PASS"

step "Log in as $CI_USER"
TOKEN=$(curl -fsS "$BASE/auth/realms/openrmf/protocol/openid-connect/token" \
  -d grant_type=password -d client_id=openrmf -d username="$CI_USER" --data-urlencode password="$CI_PASS" \
  | python3 -c 'import json,sys; print(json.load(sys.stdin)["access_token"])') || fail "could not get a token"
AUTH=(-H "Authorization: Bearer $TOKEN")
echo "token acquired (${#TOKEN} chars)"

step "Prepare the CKLB: set a host name and a few statuses"
python3 - "$FIXTURE" "$WORK/ci-rhel9.cklb" <<'PY'
import json, sys
d = json.load(open(sys.argv[1], encoding="utf-8-sig"))
d["target_data"]["host_name"] = "ci-rhel9"
rules = d["stigs"][0]["rules"]
for r in rules[:10]:  r["status"] = "open"
for r in rules[10:30]: r["status"] = "not_a_finding"
for r in rules[30:35]: r["status"] = "not_applicable"
rules[0]["overrides"] = {"severity": {"severity": "medium", "reason": "CI override test"}}
json.dump(d, open(sys.argv[2], "w", encoding="utf-8"))
print(f"{len(rules)} rules: 10 open, 20 not a finding, 5 not applicable, rest not reviewed")
PY

upload() {
  curl -fsS "${AUTH[@]}" -F "system=CI Smoke Test" ${SG:+-F "systemGroupId=$SG"} \
    -F "checklistFiles=@$WORK/ci-rhel9.cklb;filename=ci-rhel9.cklb" "$BASE/api/read/upload"
}

step "Upload the CKLB"
R1=$(upload) || fail "upload request failed"
echo "$R1"
python3 -c 'import json,sys; r=json.loads(sys.argv[1]); assert r["successful"]==1 and r["failed"]==0, r' "$R1" || fail "first upload was not accepted"

step "Find the system package and checklist"
SG=$(curl -fsS "${AUTH[@]}" "$BASE/api/read/artifact/systems" | python3 -c '
import json,sys
s=[x for x in json.load(sys.stdin) if x.get("title")=="CI Smoke Test"]
print(s[0]["internalIdString"] if s else "")')
[ -n "$SG" ] || fail "system package 'CI Smoke Test' not found"
echo "system package id: $SG"
curl -fsS "${AUTH[@]}" "$BASE/api/read/artifact/systems/$SG" -o "$WORK/system.json" || fail "could not list the system's checklists"
ART=$(python3 - "$WORK/system.json" <<'PY'
import json, sys
arts = json.load(open(sys.argv[1]))
assert len(arts) == 1, f"expected 1 checklist, found {len(arts)}"
a = arts[0]
print("checklist:", a.get("title"), "| host:", a.get("hostName"), "| type:", a.get("stigType"), "| release:", a.get("stigRelease"), file=sys.stderr)
assert a.get("hostName") == "ci-rhel9", a.get("hostName")
assert "Red Hat Enterprise Linux 9" in (a.get("stigType") or ""), a.get("stigType")
print(a["internalIdString"])
PY
) || fail "checklist record check failed"
echo "checklist id: $ART"

step "Wait for scoring (asynchronous, via NATS)"
for i in $(seq 1 24); do
  SCORE=$(curl -fsS "${AUTH[@]}" "$BASE/api/scoring/artifact/$ART" 2>/dev/null || true)
  if [ -n "$SCORE" ] && python3 - "$SCORE" <<'PY'
import json, sys
s = json.loads(sys.argv[1])
keys = ("totalOpen","totalNotAFinding","totalNotApplicable","totalNotReviewed","totalCat1Open","totalCat2Open","totalCat3Open")
total = sum(s.get(k, 0) for k in keys[:4])
if total == 0:
    sys.exit(2)  # not scored yet
print("score:", {k: s.get(k) for k in keys})
assert s.get("totalOpen") == 10, s.get("totalOpen")
assert s.get("totalNotAFinding") == 20, s.get("totalNotAFinding")
assert s.get("totalNotApplicable") == 5, s.get("totalNotApplicable")
assert total == 445, total
PY
  then SCORED=1; break
  else rc=$?; [ $rc -eq 2 ] || [ -z "$SCORE" ] || fail "score check failed"; sleep 5; fi
done
[ -n "${SCORED:-}" ] || fail "checklist was not scored within 2 minutes"

step "Download the checklist as CKL and check its content"
curl -fsS "${AUTH[@]}" "$BASE/api/read/artifact/download/$ART" -o "$WORK/download.ckl" || fail "CKL download failed"
python3 - "$WORK/download.ckl" <<'PY'
import sys, xml.etree.ElementTree as ET
root = ET.parse(sys.argv[1]).getroot()
vulns = root.iter("VULN")
vulns = list(vulns)
print("VULNs in downloaded CKL:", len(vulns))
assert len(vulns) == 445
assert root.find("ASSET/HOST_NAME").text == "ci-rhel9"
first = vulns[0]
data = {sd.findtext("VULN_ATTRIBUTE"): sd.findtext("ATTRIBUTE_DATA") for sd in first.iter("STIG_DATA")}
assert "<VulnDiscussion>" not in (data.get("Vuln_Discuss") or ""), "raw XCCDF tags leaked into Vuln_Discuss"
assert first.findtext("STATUS") == "Open", first.findtext("STATUS")
assert first.findtext("SEVERITY_OVERRIDE") == "medium", first.findtext("SEVERITY_OVERRIDE")
print("first rule:", data.get("Vuln_Num"), data.get("Rule_Ver"), "| status Open | override medium")
PY

step "Upload the same CKLB again: it must update the checklist, not add a second one"
R2=$(upload) || fail "second upload failed"
echo "$R2"
python3 -c 'import json,sys; r=json.loads(sys.argv[1]); assert r["failed"]==0 and r.get("updated",0)==1, r' "$R2" || fail "second upload did not update the existing checklist"
COUNT=$(curl -fsS "${AUTH[@]}" "$BASE/api/read/artifact/systems/$SG" | python3 -c 'import json,sys; print(len(json.load(sys.stdin)))')
[ "$COUNT" = "1" ] || fail "expected 1 checklist after re-upload, found $COUNT"

step "Reject a non-CKLB file named .cklb"
echo '{"hello":"world"}' > "$WORK/bad.cklb"
R3=$(curl -fsS "${AUTH[@]}" -F "systemGroupId=$SG" -F "system=CI Smoke Test" -F "checklistFiles=@$WORK/bad.cklb" "$BASE/api/read/upload") || fail "bad-file request errored instead of reporting a failed upload"
echo "$R3"
python3 -c 'import json,sys; r=json.loads(sys.argv[1]); assert r["failed"]==1 and r["successful"]==0, r' "$R3" || fail "bad CKLB was not reported as failed"

echo
echo "ALL SMOKE TESTS PASSED"
