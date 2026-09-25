#!/usr/bin/env bash
# Regenerates the C# sources for both packages from the OpenTelemetry semantic conventions
# registry, using OpenTelemetry Weaver in Docker.
#
# The semconv version comes from <SemanticConventionsVersion> in Directory.Build.props.
# Override the weaver image with WEAVER_IMAGE. Requires docker and jq.
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
weaver_image="${WEAVER_IMAGE:-otel/weaver:v0.26.1}"
semconv_version="$(sed -n 's:.*<SemanticConventionsVersion>\(.*\)</SemanticConventionsVersion>.*:\1:p' "$repo_root/Directory.Build.props")"

if [[ -z "$semconv_version" ]]; then
  echo "SemanticConventionsVersion not found in Directory.Build.props" >&2
  exit 1
fi

registry="https://github.com/open-telemetry/semantic-conventions.git@v${semconv_version}[model]"

generate() {
  local project="$1" stable_only="$2"
  local out="src/$project/Generated"

  echo "==> $project (semconv v$semconv_version, stable_only=$stable_only)"
  rm -rf "${repo_root:?}/$out"
  mkdir -p "$repo_root/$out"

  local diagnostics status=0
  diagnostics="$(mktemp "${TMPDIR:-/tmp}/weaver-diagnostics.XXXXXX")"

  docker run --rm \
    --user "$(id -u):$(id -g)" \
    --env HOME=/tmp \
    --mount "type=bind,source=$repo_root,target=/repo" \
    --workdir /repo \
    "$weaver_image" \
    registry generate \
      --registry "$registry" \
      --templates templates \
      --param "stable_only=$stable_only" \
      --param "semconv_version=$semconv_version" \
      --quiet \
      --diagnostic-format json \
      dotnet \
      "$out" \
    2>"$diagnostics" || status=$?

  # Show every diagnostic except the "definition/2 file format is not yet stable" warning,
  # which the upstream registry triggers for each of its files. Anything that isn't JSON
  # (e.g. a Docker error) is shown as-is.
  if jq -e . "$diagnostics" >/dev/null 2>&1; then
    jq -r '.[]? | select(.error.FailToResolveDefinition.UnstableFileFormat | not) | .diagnostic.ansi_message' "$diagnostics" >&2
  else
    cat "$diagnostics" >&2
  fi
  rm -f "$diagnostics"

  return "$status"
}

generate Nova.OpenTelemetry.SemanticConventions true
generate Nova.OpenTelemetry.SemanticConventions.Incubating false
