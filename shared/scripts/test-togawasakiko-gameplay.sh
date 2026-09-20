#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
GAME_ROOT="$(awk 'NF { print; exit }' "${REPO_ROOT}/local/game-path.txt")"
RUNTIME="${GAME_ROOT}/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64"
REFERENCES="${REPO_ROOT}/references/game-dlls/test-support"

# Only managed dependencies are copied. This never starts Steam, Godot or STS2.
cmp "${RUNTIME}/sts2.dll" "${REPO_ROOT}/references/game-dlls/sts2/arm64/sts2.dll"
mkdir -p "${REFERENCES}"
for name in Steamworks.NET Sentry SmartFormat SmartFormat.ZString; do
  cp "${RUNTIME}/${name}.dll" "${REFERENCES}/${name}.dll"
done

PROJECT="${REPO_ROOT}/mods/Togawasakiko_in_Slay_the_Spire/tests/GameplayRegression.csproj"
dotnet build "${PROJECT}" -c Release --nologo -v quiet
DOTNET="${REPO_ROOT}/local/tools/dotnet-runtime-9/dotnet"
if [[ ! -x "${DOTNET}" ]]; then
  DOTNET="$(command -v dotnet)"
fi
exec "${DOTNET}" "${REPO_ROOT}/mods/Togawasakiko_in_Slay_the_Spire/tests/bin/Release/net9.0/GameplayRegression.dll"
