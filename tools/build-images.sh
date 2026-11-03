#!/usr/bin/env bash
# build-images.sh — build the AUT0-CH container images (connected side).
#
#   tools/build-images.sh            # builds all four with VERSION below
#   VERSION=0.2.0 tools/build-images.sh api-read web
#   ENGINE=podman tools/build-images.sh
#
# Needs network access to NuGet, mcr.microsoft.com and Docker Hub (base images).
# Tags must match the localhost/aut0-ch/* entries in docs/scripts/docker-compose.yml.
set -euo pipefail

ENGINE="${ENGINE:-docker}"
VERSION="${VERSION:-0.1.0}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
if [ $# -gt 0 ]; then COMPONENTS=("$@"); else COMPONENTS=(api-read api-reports msg-reports web); fi

for c in "${COMPONENTS[@]}"; do
  [ -f "$ROOT/$c/Dockerfile" ] || { echo "No Dockerfile in $c" >&2; exit 1; }
  tag="localhost/aut0-ch/$c:$VERSION"
  echo ">> building $tag"
  "$ENGINE" build --pull -t "$tag" -f "$ROOT/$c/Dockerfile" "$ROOT/$c"
done

echo
echo "Built: ${COMPONENTS[*]} at $VERSION"
echo "If VERSION changed, update the localhost/aut0-ch/* tags in docs/scripts/docker-compose.yml."
