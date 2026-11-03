#!/usr/bin/env bash
# airgap-bundle.sh (AUT0-CH)
# Build (connected side) or load (air-gapped side) an AUT0-CH image bundle.
# Run tools/build-images.sh first so the localhost/aut0-ch/* images exist locally.
#
#   Connected:   ./tools/airgap-bundle.sh save  /path/to/docker-compose.yml  [outdir]
#   Air-gapped:  ./tools/airgap-bundle.sh load  /path/to/bundle-dir
#
# Images are read straight from docker-compose.yml so the list never drifts from
# what start.sh will actually launch. Set ENGINE=podman to use podman.
set -euo pipefail

ENGINE="${ENGINE:-docker}"
mode="${1:-}"

images_from_compose() {
  # uncommented "image:" lines only; strip quotes
  grep -E '^[[:space:]]*image:' "$1" | sed -E 's/^[[:space:]]*image:[[:space:]]*//; s/["'\'']//g' | sort -u
}

case "$mode" in
  save)
    compose="${2:?path to docker-compose.yml}"
    out="${3:-aut0-ch-bundle-$(date +%Y%m%d)}"
    mkdir -p "$out/images"
    images_from_compose "$compose" > "$out/images.txt"
    echo "Found $(wc -l < "$out/images.txt") images in $compose"
    while read -r img; do
      if [[ "$img" == localhost/* ]]; then
        "$ENGINE" image inspect "$img" >/dev/null 2>&1 || { echo "MISSING local build: $img (run tools/build-images.sh)"; exit 1; }
      else
        echo ">> pull $img"
        "$ENGINE" pull "$img"
      fi
      f="$out/images/$(echo "$img" | sed 's#^docker.io/##; s#^localhost/##; s#[/:]#_#g').tar"
      echo ">> save $img -> $f"
      "$ENGINE" save -o "$f" "$img"
    done < "$out/images.txt"
    # bring the release config files along (compose, .env, .grafana, nginx, init JS, keycloak scripts, grafana dirs)
    cp -r "$(dirname "$compose")" "$out/aut0-ch-release"
    ( cd "$out" && find images aut0-ch-release -type f -print0 | sort -z | xargs -0 sha256sum > SHA256SUMS )
    echo "Bundle ready: $out  (verify on the far side with: sha256sum -c SHA256SUMS)"
    ;;
  load)
    dir="${2:?path to bundle dir}"
    ( cd "$dir" && sha256sum -c --quiet SHA256SUMS ) && echo "Checksums OK"
    for f in "$dir"/images/*.tar; do
      echo ">> load $f"
      "$ENGINE" load -i "$f"
    done
    echo ">> verifying every compose image is now present locally"
    missing=0
    while read -r img; do
      if ! "$ENGINE" image inspect "$img" >/dev/null 2>&1; then
        echo "MISSING: $img"; missing=1
      fi
    done < "$dir/images.txt"
    [ "$missing" -eq 0 ] && echo "All images present. Edit .env/.grafana with this host's IP/DNS, then run ./start.sh"
    exit "$missing"
    ;;
  *)
    sed -n '2,9p' "$0"; exit 1 ;;
esac
