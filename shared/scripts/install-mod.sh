#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
GAME_PATH_FILE="${REPO_ROOT}/local/game-path.txt"

usage() {
  cat <<'EOF'
用法：
  ./shared/scripts/install-mod.sh <mod 名称或路径> [--apply] [--dry-run] [--create-mods-dir] [--allow-overwrite] [--replace-target]

默认行为：
  - 默认目标路径按“当前平台推导出的 mods 根目录”处理。
  - 默认是 dry-run，只显示将要执行的动作，不写入游戏目录。
  - 默认不覆盖游戏目录中已有同名 mod 目录。

选项：
  --apply             真正执行复制安装。
  --dry-run           强制 dry-run（默认已开启）。
  --create-mods-dir   当推导出的 mods 根目录不存在时，允许脚本安全创建该目录。
  --allow-overwrite   允许向已存在的同名 mod 目录写入并覆盖同名文件。
  --replace-target    先安全删除目标 mod 目录，再按最小安装包重建。
  -h, --help          显示帮助。
EOF
}

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

resolve_mods_root() {
  local game_root="$1"
  local macos_executable_dir="${game_root}/SlayTheSpire2.app/Contents/MacOS"
  local macos_executable_path="${macos_executable_dir}/Slay the Spire 2"

  if [[ -x "${macos_executable_path}" ]]; then
    printf '%s/mods\n' "${macos_executable_dir}"
    return
  fi

  printf '%s/mods\n' "${game_root}"
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

collect_release_items() {
  local release_dir="$1"

  find "${release_dir}" -mindepth 1 -maxdepth 1 ! -name ".gitkeep" -print0 | sort -z
}

warn_if_target_contains_workspace_content() {
  local target_dir="$1"
  local suspicious=0
  local candidates=(
    ".build"
    "src"
    "assets"
    "exports"
    "manifest"
    "README.md"
  )

  [[ -d "${target_dir}" ]] || return 0

  for name in "${candidates[@]}"; do
    if [[ -e "${target_dir}/${name}" ]]; then
      suspicious=1
      break
    fi
  done

  if [[ "${suspicious}" -eq 1 ]]; then
    echo "警告：目标目录中发现开发区样式内容，可能已混入不应安装到游戏目录的文件：${target_dir}"
    echo "脚本不会自动删除这些内容，请人工确认是否需要清理。"
  fi
}

safe_remove_target_dir() {
  local target_dir="$1"
  local mods_root="$2"

  if [[ ! -d "${target_dir}" ]]; then
    return 0
  fi

  case "${target_dir}" in
    "${mods_root}"/*) ;;
    *)
      echo "拒绝删除 mods 根目录外路径：${target_dir}" >&2
      exit 1
      ;;
  esac

  if [[ "${target_dir}" == "${mods_root}" ]]; then
    echo "拒绝删除 mods 根目录本身：${mods_root}" >&2
    exit 1
  fi

  rm -rf "${target_dir}"
}

resolve_package_source_dir() {
  local release_dir="$1"
  local mod_name="$2"
  local nested_dir="${release_dir}/${mod_name}"

  if [[ -d "${nested_dir}" ]]; then
    local nested_count
    nested_count="$(find "${nested_dir}" -mindepth 1 -maxdepth 1 ! -name ".gitkeep" | wc -l | tr -d ' ')"
    if [[ "${nested_count}" != "0" ]]; then
      printf '%s\n' "${nested_dir}"
      return
    fi
  fi

  printf '%s\n' "${release_dir}"
}

preview_copy_plan() {
  local source_dir="$1"
  local target_dir="$2"
  local allow_overwrite="$3"

  while IFS= read -r -d '' item; do
    local name dest_path status
    name="$(basename "${item}")"
    dest_path="${target_dir}/${name}"

    if [[ -e "${dest_path}" ]]; then
      if [[ "${allow_overwrite}" -eq 1 ]]; then
        status="覆盖候选"
      else
        status="已存在，默认跳过"
      fi
    else
      status="将新增"
    fi

    echo "  [${status}] ${dest_path}"
  done < <(collect_release_items "${source_dir}")
}

copy_release_files() {
  local source_dir="$1"
  local target_dir="$2"
  local allow_overwrite="$3"

  mkdir -p "${target_dir}"

  while IFS= read -r -d '' item; do
    local name dest_path
    name="$(basename "${item}")"
    dest_path="${target_dir}/${name}"

    if [[ -d "${item}" ]]; then
      if [[ -e "${dest_path}" && "${allow_overwrite}" -ne 1 ]]; then
        echo "跳过已存在目录：${dest_path}"
        continue
      fi

      if [[ -e "${dest_path}" && "${allow_overwrite}" -eq 1 ]]; then
        mkdir -p "${dest_path}"
        cp -Rfv "${item}/." "${dest_path}/"
      else
        cp -Rv "${item}" "${dest_path}"
      fi
      continue
    fi

    if [[ -e "${dest_path}" && "${allow_overwrite}" -ne 1 ]]; then
      echo "跳过已存在文件：${dest_path}"
      continue
    fi

    if [[ "${allow_overwrite}" -eq 1 ]]; then
      cp -fv "${item}" "${dest_path}"
    else
      cp -v "${item}" "${dest_path}"
    fi
  done < <(collect_release_items "${source_dir}")
}

MOD_INPUT=""
APPLY=0
CREATE_MODS_DIR=0
ALLOW_OVERWRITE=0
REPLACE_TARGET=0

while [[ $# -gt 0 ]]; do
  case "$1" in
    --apply)
      APPLY=1
      ;;
    --dry-run)
      APPLY=0
      ;;
    --create-mods-dir)
      CREATE_MODS_DIR=1
      ;;
    --allow-overwrite)
      ALLOW_OVERWRITE=1
      ;;
    --replace-target)
      REPLACE_TARGET=1
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      if [[ -z "${MOD_INPUT}" ]]; then
        MOD_INPUT="$1"
      else
        echo "无法识别的额外参数：$1" >&2
        usage
        exit 1
      fi
      ;;
  esac
  shift || true
done

if [[ -z "${MOD_INPUT}" ]]; then
  usage
  exit 1
fi

GAME_ROOT="$(read_game_root)"
MOD_DIR="$(resolve_mod_dir "${MOD_INPUT}")"
MOD_NAME="$(basename "${MOD_DIR}")"
RELEASE_DIR="${MOD_DIR}/exports/release"
PACKAGE_SOURCE_DIR="$(resolve_package_source_dir "${RELEASE_DIR}" "${MOD_NAME}")"
MODS_ROOT="$(resolve_mods_root "${GAME_ROOT}")"
TARGET_DIR="${MODS_ROOT}/${MOD_NAME}"

if [[ ! -d "${GAME_ROOT}" ]]; then
  echo "游戏根目录不存在：${GAME_ROOT}" >&2
  exit 1
fi

if [[ ! -d "${RELEASE_DIR}" ]]; then
  echo "未找到 release 目录：${RELEASE_DIR}"
  echo "当前没有可安装成品。"
  exit 0
fi

RELEASE_ITEMS=()
while IFS= read -r -d '' item; do
  RELEASE_ITEMS+=("${item}")
done < <(collect_release_items "${PACKAGE_SOURCE_DIR}")

if [[ "${#RELEASE_ITEMS[@]}" -eq 0 ]]; then
  echo "mod：${MOD_NAME}"
  echo "候选安装包目录为空：${PACKAGE_SOURCE_DIR}"
  echo "当前没有可安装成品，脚本安全退出。"
  exit 0
fi

echo "mod：${MOD_NAME}"
echo "游戏根目录：${GAME_ROOT}"
echo "release 目录：${RELEASE_DIR}"
echo "候选安装包目录：${PACKAGE_SOURCE_DIR}"
echo "候选安装根目录：${MODS_ROOT}"
echo "候选安装目录：${TARGET_DIR}"
echo "当前模式：$([[ "${APPLY}" -eq 1 ]] && echo 'apply' || echo 'dry-run')"
if [[ "${REPLACE_TARGET}" -eq 1 ]]; then
  echo "目标处理：replace-target"
fi
echo
echo "检测到以下 release 成品："
printf '  %s\n' "${RELEASE_ITEMS[@]}"
echo

if [[ ! -d "${MODS_ROOT}" ]]; then
  echo "检测结果：推导出的 mods 根目录当前不存在。"
  echo "这与“当前平台的真实扫描目录仍需继续验证”的状态一致。"
  echo
  if [[ "${APPLY}" -ne 1 ]]; then
    echo "当前为 dry-run，不会创建目录。"
    echo "如要按当前推导出的目标目录继续验证，可执行："
    echo "  ./shared/scripts/install-mod.sh \"${MOD_INPUT}\" --apply --create-mods-dir"
    exit 0
  fi

  if [[ "${CREATE_MODS_DIR}" -ne 1 ]]; then
    echo "拒绝直接写入，因为推导出的 mods 根目录尚不存在。"
    echo "如确认要按当前推导出的目录继续验证，请显式追加 --create-mods-dir。"
    exit 1
  fi

  echo "将安全创建候选 mods 目录：${MODS_ROOT}"
  mkdir -p "${MODS_ROOT}"
fi

echo "目标目录存在状态：$([[ -d "${TARGET_DIR}" ]] && echo '已存在' || echo '不存在')"
warn_if_target_contains_workspace_content "${TARGET_DIR}"

if [[ -d "${TARGET_DIR}" && "${REPLACE_TARGET}" -eq 1 && "${APPLY}" -ne 1 ]]; then
  echo "dry-run 提示：将删除并重建目标目录：${TARGET_DIR}"
fi

if [[ -d "${TARGET_DIR}" && "${ALLOW_OVERWRITE}" -ne 1 ]]; then
  if [[ "${REPLACE_TARGET}" -eq 1 ]]; then
    :
  else
  echo "默认策略：不覆盖已存在的同名 mod 目录。"
  echo "如确认要覆盖同名文件，请显式追加 --allow-overwrite。"
  echo
  echo "dry-run 预览："
  preview_copy_plan "${PACKAGE_SOURCE_DIR}" "${TARGET_DIR}" "${ALLOW_OVERWRITE}"
  exit 0
  fi
fi

echo "dry-run 预览："
preview_copy_plan "${PACKAGE_SOURCE_DIR}" "${TARGET_DIR}" "${ALLOW_OVERWRITE}"
echo

if [[ "${APPLY}" -ne 1 ]]; then
  echo "当前为 dry-run，不会写入游戏目录。"
  echo "如确认预览结果正确，可执行："
  if [[ "${REPLACE_TARGET}" -eq 1 ]]; then
    echo "  ./shared/scripts/install-mod.sh \"${MOD_INPUT}\" --apply --replace-target"
  elif [[ "${ALLOW_OVERWRITE}" -eq 1 ]]; then
    echo "  ./shared/scripts/install-mod.sh \"${MOD_INPUT}\" --apply --allow-overwrite"
  else
    echo "  ./shared/scripts/install-mod.sh \"${MOD_INPUT}\" --apply"
  fi
  exit 0
fi

if [[ "${REPLACE_TARGET}" -eq 1 ]]; then
  safe_remove_target_dir "${TARGET_DIR}" "${MODS_ROOT}"
fi

copy_release_files "${PACKAGE_SOURCE_DIR}" "${TARGET_DIR}" "${ALLOW_OVERWRITE}"
echo
echo "安装完成。"
if [[ "${REPLACE_TARGET}" -eq 1 ]]; then
  echo "本次已精确替换目标目录，只保留当前最小安装包文件。"
elif [[ "${ALLOW_OVERWRITE}" -eq 1 ]]; then
  echo "本次允许覆盖同名文件，但不会删除目标目录中的额外旧文件。"
else
  echo "本次未覆盖任何已存在的同名文件。"
fi
echo "请启动游戏验证当前推导出的 mods 目录是否为本机正确安装路径。"
