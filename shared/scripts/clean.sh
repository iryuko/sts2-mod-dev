#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

assert_repo_path() {
  case "$1" in
    "${REPO_ROOT}"/*) ;;
    *)
      echo "拒绝清理仓库外路径：$1" >&2
      exit 1
      ;;
  esac
}

remove_path() {
  local target="$1"
  assert_repo_path "${target}"

  if [[ -e "${target}" ]]; then
    echo "删除：${target}"
    rm -rf "${target}"
  fi
}

SAFE_DIRS=()
while IFS= read -r dir; do
  SAFE_DIRS+=("${dir}")
done < <(find "${REPO_ROOT}/mods" -type d \( -name bin -o -name obj -o -name .godot \) | sort)
if [[ ${#SAFE_DIRS[@]} -gt 0 ]]; then
  for dir in "${SAFE_DIRS[@]}"; do
    remove_path "${dir}"
  done
fi

EXPORT_ITEMS=()
while IFS= read -r item; do
  EXPORT_ITEMS+=("${item}")
done < <(find "${REPO_ROOT}/mods" \( -path "*/exports/debug/*" -o -path "*/exports/release/*" \) ! -name ".gitkeep" | sort)
if [[ ${#EXPORT_ITEMS[@]} -gt 0 ]]; then
  for item in "${EXPORT_ITEMS[@]}"; do
    remove_path "${item}"
  done
fi

echo "清理完成。"
echo "本脚本只处理工作区内的导出产物和临时目录，不会接触游戏根目录。"
