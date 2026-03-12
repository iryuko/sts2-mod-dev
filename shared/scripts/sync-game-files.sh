#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
GAME_PATH_FILE="${REPO_ROOT}/local/game-path.txt"
REFERENCES_DIR="${REPO_ROOT}/references"

read_game_root() {
  if [[ ! -f "${GAME_PATH_FILE}" ]]; then
    echo "未找到游戏路径配置：${GAME_PATH_FILE}" >&2
    exit 1
  fi

  local game_root
  game_root="$(awk 'NF { print; exit }' "${GAME_PATH_FILE}")"

  if [[ -z "${game_root}" ]]; then
    echo "游戏路径配置为空：${GAME_PATH_FILE}" >&2
    exit 1
  fi

  printf '%s\n' "${game_root}"
}

GAME_ROOT="$(read_game_root)"

if [[ ! -d "${GAME_ROOT}" ]]; then
  echo "游戏根目录不存在：${GAME_ROOT}" >&2
  exit 1
fi

copy_reference_file() {
  local source_path="$1"
  local target_path="$2"

  if [[ ! -f "${source_path}" ]]; then
    echo "  [缺失] ${source_path}"
    return
  fi

  mkdir -p "$(dirname "${target_path}")"
  cp -f "${source_path}" "${target_path}"
  echo "  [已复制] ${source_path}"
  echo "           -> ${target_path}"
}

echo "已读取游戏根目录：${GAME_ROOT}"
echo "当前脚本只会从游戏目录复制少量参考文件到工作区，不会修改游戏本体。"
echo
echo "已观察到的关键路径："

candidate_paths=(
  "${GAME_ROOT}/SlayTheSpire2.app"
  "${GAME_ROOT}/SlayTheSpire2.app/Contents"
  "${GAME_ROOT}/SlayTheSpire2.app/Contents/MacOS"
  "${GAME_ROOT}/SlayTheSpire2.app/Contents/Resources"
  "${GAME_ROOT}/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64"
  "${GAME_ROOT}/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_x86_64"
  "${GAME_ROOT}/controller_config"
)

for path in "${candidate_paths[@]}"; do
  if [[ -e "${path}" ]]; then
    echo "  [存在] ${path}"
  else
    echo "  [待确认] ${path}"
  fi
done

echo
echo "开始复制关键参考文件："

copy_reference_file \
  "${GAME_ROOT}/SlayTheSpire2.app/Contents/Info.plist" \
  "${REFERENCES_DIR}/api-notes/app/Info.plist"
copy_reference_file \
  "${GAME_ROOT}/SlayTheSpire2.app/Contents/Resources/release_info.json" \
  "${REFERENCES_DIR}/api-notes/app/release_info.json"
copy_reference_file \
  "${GAME_ROOT}/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.runtimeconfig.json" \
  "${REFERENCES_DIR}/api-notes/sts2/sts2.runtimeconfig.arm64.json"
copy_reference_file \
  "${GAME_ROOT}/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.deps.json" \
  "${REFERENCES_DIR}/api-notes/sts2/sts2.deps.arm64.json"
copy_reference_file \
  "${GAME_ROOT}/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_x86_64/sts2.deps.json" \
  "${REFERENCES_DIR}/api-notes/sts2/sts2.deps.x86_64.json"
copy_reference_file \
  "${GAME_ROOT}/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/0Harmony.dll" \
  "${REFERENCES_DIR}/game-dlls/shared/0Harmony.dll"
copy_reference_file \
  "${GAME_ROOT}/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/GodotSharp.dll" \
  "${REFERENCES_DIR}/game-dlls/shared/GodotSharp.dll"
copy_reference_file \
  "${GAME_ROOT}/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll" \
  "${REFERENCES_DIR}/game-dlls/sts2/arm64/sts2.dll"
copy_reference_file \
  "${GAME_ROOT}/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_x86_64/sts2.dll" \
  "${REFERENCES_DIR}/game-dlls/sts2/x86_64/sts2.dll"

echo
echo "以下大文件或运行文件当前只记录路径，不复制："
echo "  [仅记录] ${GAME_ROOT}/SlayTheSpire2.app/Contents/MacOS/Slay the Spire 2"
echo "  [仅记录] ${GAME_ROOT}/SlayTheSpire2.app/Contents/Resources/Slay the Spire 2.pck"
echo
echo "同步完成。后续如需新增参考文件，仍应先确认用途，再加入白名单。"
