#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PROJECT="${REPO_ROOT}/mods/Togawasakiko_in_Slay_the_Spire/tests/godot"
GODOT_MONO="${GODOT_MONO:-${REPO_ROOT}/local/tools/godot-mono-4.5.1/Godot_mono.app/Contents/MacOS/Godot}"
if [[ ! -x "${GODOT_MONO}" ]]; then
  printf '%s\n' 'Set GODOT_MONO to a Godot 4.5.1 .NET executable.' >&2
  exit 1
fi
if [[ -x "${REPO_ROOT}/local/tools/dotnet-runtime-9/dotnet" ]]; then
  export DOTNET_ROOT="${REPO_ROOT}/local/tools/dotnet-runtime-9"
fi

dotnet build "${PROJECT}/ResourceLifecycle.csproj" --nologo -v quiet
LOG="$(mktemp)"
trap 'rm -f "$LOG"' EXIT
"${GODOT_MONO}" --headless --audio-driver Dummy --path "${PROJECT}" \
  --max-fps 60 --quit-after 1200 2>&1 | tee "${LOG}"
# Godot's frame-limit exit is successful even when a test never finishes.
grep -q '^RESULT native resource and event regression passed$' "${LOG}"
if grep -Eq '^ERROR:|^WARNING:.*(leaked|still in use)' "${LOG}"; then
  exit 1
fi
