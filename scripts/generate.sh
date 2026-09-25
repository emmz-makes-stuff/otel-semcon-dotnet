#!/usr/bin/env bash
# Generates the C# sources for the packages from the OpenTelemetry semantic conventions
# registry, using OpenTelemetry Weaver in Docker. Requires docker and jq.
#
#   scripts/generate.sh                               # both projects
#   scripts/generate.sh SemanticConventions.Incubating  # one project (what the build runs)
#
# Reads <SemanticConventionsVersion> from Directory.Build.props and
# <SemanticConventionsRootNamespace> from Package.props. Override the weaver image with
# WEAVER_IMAGE.
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
weaver_image="${WEAVER_IMAGE:-otel/weaver:v0.26.1}"

# Reads a single-line <Name>value</Name> MSBuild property from a file.
read_property() {
  local name="$1" file="$2" value
  value="$(sed -n "s:.*<$name>\(.*\)</$name>.*:\1:p" "$repo_root/$file" | head -n 1)"
  if [[ -z "$value" ]]; then
    echo "$name not found in $file" >&2
    exit 1
  fi
  echo "$value"
}

for tool in docker jq; do
  if ! command -v "$tool" >/dev/null 2>&1; then
    echo "generate.sh needs $tool on the PATH to run OpenTelemetry Weaver." >&2
    exit 1
  fi
done

semconv_version="$(read_property SemanticConventionsVersion Directory.Build.props)"
root_namespace="$(read_property SemanticConventionsRootNamespace Package.props)"
registry="https://github.com/open-telemetry/semantic-conventions.git@v${semconv_version}[model]"

generate() {
  local project="$1" stable_only
  local out="src/$project/Generated"

  case "$project" in
    SemanticConventions) stable_only=true ;;
    SemanticConventions.Incubating) stable_only=false ;;
    *) echo "Unknown project: $project" >&2; return 1 ;;
  esac

  echo "==> $project: $root_namespace (semconv v$semconv_version, stable_only=$stable_only)"
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
      --param "root_namespace=$root_namespace" \
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

if [[ $# -gt 0 ]]; then
  for project in "$@"; do generate "$project"; done
else
  generate SemanticConventions
  generate SemanticConventions.Incubating
fi
