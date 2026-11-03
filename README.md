# AUT0-CH
**Another Useless Tool Zero – Cyber Hygiene**

AUT0-CH is a self-hosted web tool for managing DISA STIG checklists, SCAP results and ACAS/Nessus scans, and for producing RMF artifacts (compliance listing, POA&M, Test Plan, RAR). It is built for closed and air-gapped networks.

It is a **modified version of [OpenRMF® OSS](https://github.com/Cingulara)** (GPLv3) and is not affiliated with or endorsed by its authors. See [NOTICE](NOTICE) for attribution and the list of changes.

## Repository layout

| Folder | What it is | Upstream |
|---|---|---|
| `docs/` | Install docs, `docker-compose.yml`, nginx/Prometheus/Grafana config, Keycloak setup scripts | Cingulara/openrmf-docs |
| `api-read/` | Main API: upload/parse CKL, SCAP and Nessus; editing; Excel exports (checklist, POA&M, Test Plan) | Cingulara/openrmf-api-read |
| `api-reports/` | Reporting API | Cingulara/openrmf-api-reports |
| `msg-reports/` | NATS client that builds the reporting data (includes the second Nessus parser) | Cingulara/openrmf-msg-reports |
| `web/` | NGINX + static web UI | Cingulara/openrmf-web |
| `tools/` | `build-images.sh`, `airgap-bundle.sh` | — |

All other services (scoring, template, controls, audit APIs; score/compliance/controls/template/system/audit message clients; MongoDB; NATS; Keycloak; Postgres; Prometheus; Grafana) run as **stock upstream images**. DISA template updates therefore still arrive by pulling newer upstream `api-template` / `msg-template` images.

## Build and deploy

Connected staging machine (Docker or Podman, network access to NuGet and Docker Hub):

```bash
tools/build-images.sh                                   # builds localhost/aut0-ch/{api-read,api-reports,msg-reports,web}:0.1.0
tools/airgap-bundle.sh save docs/scripts/docker-compose.yml   # saves all 23 images + config + SHA256SUMS
```

Air-gapped host:

```bash
tools/airgap-bundle.sh load <bundle-dir>
cd <bundle-dir>/aut0-ch-release
# set host IP/DNS in .env and .grafana, then:
./start.sh
# after 2–3 minutes:
sudo keycloak/setup-realm-linux.sh
```

Then follow `docs/step-by-step.md` steps 5–7 (Keycloak redirect URIs and first user) and `docs/securing-openrmf.md` (change every default password).

## Compatibility rules (do not rename)

These identifiers are shared with the stock upstream services or with existing data, so they keep their `openrmf` names:

- NATS subjects (`openrmf.checklist.save.new`, …)
- MongoDB database and user names, and `docs/scripts/initialize*.js`
- Keycloak realm and client id `openrmf`, and role names
- Compose service/container names (`checklistdb`, `natsserver`, `openrmfapi-*`, `openrmf-web`, `openrmf-keycloak`)
- C# namespaces and Prometheus metric names

## Pulling upstream changes

Each folder is a `git subtree` with an `upstream-<folder>` remote:

```bash
git fetch upstream-api-read master
git subtree pull --prefix=api-read upstream-api-read master --squash -m "Merge upstream openrmf-api-read"
```

(The same applies to `docs`, `api-reports`, `msg-reports`, and `web`.) Re-check `NOTICE` and the compose image tags after each upstream merge.

## Roadmap

1. `.cklb` import (CKLB → CKL at upload), with `.cklb` added to the web upload filter
2. Nessus: better host identity than masked IPs, credentialed vs. uncredentialed separation, auth-failure flags, POA&M control numbers
3. Excel exports driven by templates mounted from a volume instead of hardcoded C#
4. Multiple scans per system package (GridFS); CKLB export with UUIDs preserved

## License

GPLv3 — see [LICENSE](LICENSE). Every upstream source file keeps its original notices.
