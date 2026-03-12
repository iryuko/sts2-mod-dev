#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
BACKUP_ROOT="${REPO_ROOT}/local/backups/mod-loading-consent"
TIMESTAMP="$(date '+%Y%m%d-%H%M%S')"

usage() {
  cat <<'EOF'
用法：
  ./shared/scripts/set-mod-loading-consent.sh [--apply] [--dry-run]

说明：
  - 默认 dry-run，只展示将更新哪些 settings.save。
  - --apply 时会先把命中的 settings.save 备份到工作区，再写入：
      .mod_settings.mods_enabled = true
      .mod_settings.disabled_mods = 现有值或 []
EOF
}

APPLY=0

while [[ $# -gt 0 ]]; do
  case "$1" in
    --apply)
      APPLY=1
      ;;
    --dry-run)
      APPLY=0
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "无法识别的参数：$1" >&2
      usage
      exit 1
      ;;
  esac
  shift || true
done

collect_settings_files() {
  find "${HOME}/Library/Application Support/SlayTheSpire2/steam" \
    -type f -name 'settings.save' 2>/dev/null | sort
  find "${HOME}/Library/Application Support/Steam/userdata" \
    -path '*/2868840/remote/settings.save' -type f 2>/dev/null | sort
}

SETTINGS_FILES=()
while IFS= read -r file; do
  [[ -n "${file}" ]] || continue
  SETTINGS_FILES+=("${file}")
done < <(collect_settings_files)

if [[ "${#SETTINGS_FILES[@]}" -eq 0 ]]; then
  echo "未找到任何 settings.save。"
  exit 1
fi

echo "当前模式：$([[ "${APPLY}" -eq 1 ]] && echo 'apply' || echo 'dry-run')"
echo "将处理以下 settings.save："
printf '  %s\n' "${SETTINGS_FILES[@]}"
echo

for settings_file in "${SETTINGS_FILES[@]}"; do
  echo "文件：${settings_file}"
  echo "当前 mod_settings："
  jq '.mod_settings' "${settings_file}"
  echo
done

if [[ "${APPLY}" -ne 1 ]]; then
  echo "当前为 dry-run，不会写入文件。"
  echo "如确认要继续，可执行："
  echo "  ./shared/scripts/set-mod-loading-consent.sh --apply"
  exit 0
fi

mkdir -p "${BACKUP_ROOT}/${TIMESTAMP}"

for settings_file in "${SETTINGS_FILES[@]}"; do
  base_name="$(basename "$(dirname "${settings_file}")")-$(basename "${settings_file}")"
  backup_path="${BACKUP_ROOT}/${TIMESTAMP}/${base_name}"
  temp_path="${settings_file}.tmp"

  cp -f "${settings_file}" "${backup_path}"

  jq '
    .mod_settings =
      ((.mod_settings // {})
      + {mods_enabled: true}
      + {disabled_mods: ((.mod_settings // {} | .disabled_mods) // [])})
  ' "${settings_file}" > "${temp_path}"

  mv "${temp_path}" "${settings_file}"
  echo "已更新：${settings_file}"
  echo "备份：${backup_path}"
done

echo
echo "写入完成。"
