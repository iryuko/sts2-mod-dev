#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

usage() {
  cat <<'EOF'
用法：
  ./shared/scripts/package.sh <mod 名称或路径>

说明：
  - 当前脚本用于预留未来的发布打包流程。
  - 现阶段默认只检查 release 目录是否已有可打包产物。
EOF
}

resolve_mod_dir() {
  local input="$1"

  if [[ -d "${input}" ]]; then
    cd "${input}" && pwd
    return
  fi

  if [[ -d "${REPO_ROOT}/mods/${input}" ]]; then
    cd "${REPO_ROOT}/mods/${input}" && pwd
    return
  fi

  echo "未找到 mod 目录：${input}" >&2
  exit 1
}

if [[ $# -lt 1 ]]; then
  usage
  exit 1
fi

MOD_DIR="$(resolve_mod_dir "$1")"
MOD_NAME="$(basename "${MOD_DIR}")"
RELEASE_DIR="${MOD_DIR}/exports/release"

if [[ ! -d "${RELEASE_DIR}" ]]; then
  echo "未找到 release 目录：${RELEASE_DIR}"
  echo "当前没有可打包成品。"
  exit 0
fi

RELEASE_ITEMS=()
while IFS= read -r item; do
  RELEASE_ITEMS+=("${item}")
done < <(find "${RELEASE_DIR}" -mindepth 1 -maxdepth 1 ! -name ".gitkeep" | sort)

if [[ "${#RELEASE_ITEMS[@]}" -eq 0 ]]; then
  echo "mod：${MOD_NAME}"
  echo "release 目录为空，当前没有可打包成品。"
  exit 0
fi

echo "mod：${MOD_NAME}"
echo "检测到以下 release 产物："
printf '  %s\n' "${RELEASE_ITEMS[@]}"
echo
echo "当前 package.sh 仅做预检查。"
echo "后续确认发布格式后，可在这里增加 zip、清单校验和版本号命名规则。"
