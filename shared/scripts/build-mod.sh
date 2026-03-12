#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
LOCAL_DOTNET="${REPO_ROOT}/local/tools/dotnet/dotnet"
LOCAL_GODOT="${REPO_ROOT}/local/tools/godot-4.5.1/Godot.app/Contents/MacOS/Godot"
GAME_PATH_FILE="${REPO_ROOT}/local/game-path.txt"

usage() {
  cat <<'EOF'
用法：
  ./shared/scripts/build-mod.sh <mod 名称或路径> [--configuration Release|Debug]

说明：
  - 当前脚本只负责构建工作区内的 .NET mod 程序集。
  - 它不会写入游戏目录。
  - 现阶段默认把构建出的主 DLL 复制到 exports/<configuration>/<ModName>/。
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

find_dotnet() {
  if [[ -x "${LOCAL_DOTNET}" ]]; then
    printf '%s\n' "${LOCAL_DOTNET}"
    return
  fi

  if command -v dotnet >/dev/null 2>&1; then
    command -v dotnet
    return
  fi

  return 1
}

find_csc() {
  if command -v csc >/dev/null 2>&1; then
    command -v csc
    return
  fi

  return 1
}

read_game_root() {
  [[ -f "${GAME_PATH_FILE}" ]] || return 1

  local game_root
  game_root="$(awk 'NF { print; exit }' "${GAME_PATH_FILE}")"
  [[ -n "${game_root}" ]] || return 1

  printf '%s\n' "${game_root}"
}

resolve_runtime_dir() {
  local game_root="$1"
  local runtime_dir="${game_root}/SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64"

  [[ -d "${runtime_dir}" ]] || return 1

  printf '%s\n' "${runtime_dir}"
}

collect_csproj() {
  local mod_dir="$1"
  find "${mod_dir}/src" -maxdepth 2 -type f -name '*.csproj' | sort
}

collect_cs_files() {
  local mod_dir="$1"
  find "${mod_dir}/src" -type f -name '*.cs' | sort
}

find_godot() {
  if [[ -x "${LOCAL_GODOT}" ]]; then
    printf '%s\n' "${LOCAL_GODOT}"
    return
  fi

  if command -v godot >/dev/null 2>&1; then
    command -v godot
    return
  fi

  if command -v godot4 >/dev/null 2>&1; then
    command -v godot4
    return
  fi

  return 1
}

resolve_pack_project_dir() {
  local mod_dir="$1"

  if [[ -d "${mod_dir}/pack" ]]; then
    printf '%s\n' "${mod_dir}/pack"
    return
  fi

  if [[ -d "${mod_dir}/assets/pck-src" ]]; then
    printf '%s\n' "${mod_dir}/assets/pck-src"
    return
  fi

  printf '%s\n' "${mod_dir}/pack"
}

collect_mcs_refs() {
  local mod_dir="$1"
  local refs_file="${mod_dir}/src/mcs-refs.txt"

  [[ -f "${refs_file}" ]] || return 0

  while IFS= read -r raw_line; do
    local line ref_path
    line="${raw_line#"${raw_line%%[![:space:]]*}"}"
    line="${line%"${line##*[![:space:]]}"}"
    [[ -z "${line}" ]] && continue
    [[ "${line}" == \#* ]] && continue

    if [[ "${line}" = /* ]]; then
      ref_path="${line}"
    else
      ref_path="${REPO_ROOT}/${line}"
    fi

    if [[ ! -f "${ref_path}" ]]; then
      echo "mcs 引用不存在：${ref_path}" >&2
      exit 1
    fi

    printf '%s\n' "${ref_path}"
  done < "${refs_file}"
}

collect_csc_runtime_refs() {
  local runtime_dir="$1"

  find "${runtime_dir}" -maxdepth 1 -type f \
    \( -name 'System*.dll' -o -name 'mscorlib.dll' -o -name 'netstandard.dll' \) \
    | sort
}

MOD_INPUT=""
CONFIGURATION="Release"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --configuration)
      shift
      CONFIGURATION="${1:-}"
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

MOD_DIR="$(resolve_mod_dir "${MOD_INPUT}")"
MOD_NAME="$(basename "${MOD_DIR}")"
BUILD_ROOT="${MOD_DIR}/.build/${CONFIGURATION}"
EXPORT_ROOT="${MOD_DIR}/exports/$(echo "${CONFIGURATION}" | tr '[:upper:]' '[:lower:]')/${MOD_NAME}"
PACK_PROJECT_DIR="$(resolve_pack_project_dir "${MOD_DIR}")"
PACK_SCRIPT_PATH="${PACK_PROJECT_DIR}/pack_pck.gd"
PCK_OUTPUT_PATH="${EXPORT_ROOT}/${MOD_NAME}.pck"

CSPROJ_FILES=()
while IFS= read -r file; do
  CSPROJ_FILES+=("${file}")
done < <(collect_csproj "${MOD_DIR}")

if [[ "${#CSPROJ_FILES[@]}" -eq 0 ]]; then
  echo "未找到 csproj：${MOD_DIR}/src"
  echo "将尝试直接编译 src 下的 .cs 文件。"
fi

if [[ "${#CSPROJ_FILES[@]}" -gt 1 ]]; then
  echo "当前脚本要求每个 mod 只有一个待构建 csproj，实际找到 ${#CSPROJ_FILES[@]} 个：" >&2
  printf '  %s\n' "${CSPROJ_FILES[@]}" >&2
  exit 1
fi

ASSEMBLY_PATH="${BUILD_ROOT}/${MOD_NAME}.dll"
DOTNET_BIN=""
CSC_BIN=""
BUILD_MODE=""

if DOTNET_BIN="$(find_dotnet)"; then
  BUILD_MODE="dotnet"
elif CSC_BIN="$(find_csc)"; then
  BUILD_MODE="csc"
elif command -v mcs >/dev/null 2>&1; then
  BUILD_MODE="mcs"
else
  echo "未找到 dotnet、csc 或 mcs，当前无法生成真实 DLL。" >&2
  exit 1
fi

echo "mod：${MOD_NAME}"
echo "构建模式：${BUILD_MODE}"
if [[ "${BUILD_MODE}" == "dotnet" ]]; then
  echo "dotnet：${DOTNET_BIN}"
  echo "项目文件：${CSPROJ_FILES[0]}"
elif [[ "${BUILD_MODE}" == "csc" ]]; then
  echo "csc：${CSC_BIN}"
fi
echo "构建配置：${CONFIGURATION}"
echo "中间输出：${BUILD_ROOT}"
echo "导出目录：${EXPORT_ROOT}"
echo "PCK 源目录：${PACK_PROJECT_DIR}"

rm -rf "${BUILD_ROOT}"
mkdir -p "${BUILD_ROOT}" "${EXPORT_ROOT}"

if [[ "${BUILD_MODE}" == "dotnet" ]]; then
  "${DOTNET_BIN}" build "${CSPROJ_FILES[0]}" -c "${CONFIGURATION}" -o "${BUILD_ROOT}" >/dev/null
elif [[ "${BUILD_MODE}" == "csc" ]]; then
  CS_FILES=()
  while IFS= read -r file; do
    CS_FILES+=("${file}")
  done < <(collect_cs_files "${MOD_DIR}")
  if [[ "${#CS_FILES[@]}" -eq 0 ]]; then
    echo "未找到可编译的 .cs 文件：${MOD_DIR}/src" >&2
    exit 1
  fi

  GAME_ROOT=""
  if ! GAME_ROOT="$(read_game_root)"; then
    echo "未找到游戏目录配置，csc fallback 无法定位 .NET 运行库：${GAME_PATH_FILE}" >&2
    exit 1
  fi

  RUNTIME_DIR=""
  if ! RUNTIME_DIR="$(resolve_runtime_dir "${GAME_ROOT}")"; then
    echo "未找到 macOS arm64 运行库目录：${GAME_ROOT}" >&2
    exit 1
  fi

  CSC_REFS=("-nostdlib+")
  while IFS= read -r runtime_ref; do
    CSC_REFS+=("-r:${runtime_ref}")
  done < <(collect_csc_runtime_refs "${RUNTIME_DIR}")
  while IFS= read -r ref_file; do
    CSC_REFS+=("-r:${ref_file}")
  done < <(collect_mcs_refs "${MOD_DIR}")

  "${CSC_BIN}" -nologo -target:library -langversion:latest -nullable:enable -debug:portable -deterministic -out:"${ASSEMBLY_PATH}" "${CSC_REFS[@]}" "${CS_FILES[@]}"
else
  CS_FILES=()
  while IFS= read -r file; do
    CS_FILES+=("${file}")
  done < <(collect_cs_files "${MOD_DIR}")
  if [[ "${#CS_FILES[@]}" -eq 0 ]]; then
    echo "未找到可编译的 .cs 文件：${MOD_DIR}/src" >&2
    exit 1
  fi

  MCS_REFS=()
  while IFS= read -r ref_file; do
    MCS_REFS+=("-r:${ref_file}")
  done < <(collect_mcs_refs "${MOD_DIR}")

  if [[ "${#MCS_REFS[@]}" -gt 0 ]]; then
    mcs -target:library -out:"${ASSEMBLY_PATH}" "${MCS_REFS[@]}" "${CS_FILES[@]}"
  else
    mcs -target:library -out:"${ASSEMBLY_PATH}" "${CS_FILES[@]}"
  fi
fi

if [[ ! -f "${ASSEMBLY_PATH}" ]]; then
  echo "构建完成但未找到主 DLL：${ASSEMBLY_PATH}" >&2
  exit 1
fi

cp -f "${ASSEMBLY_PATH}" "${EXPORT_ROOT}/${MOD_NAME}.dll"

mkdir -p "${PACK_PROJECT_DIR}"

PROJECT_GODOT_PATH="${PACK_PROJECT_DIR}/project.godot"
cat > "${PROJECT_GODOT_PATH}" <<EOF
config_version=5

[application]
config/name="${MOD_NAME}"
config/features=PackedStringArray("4.5", "Forward Plus")
run/main_scene=""

[dotnet]
project/assembly_name="${MOD_NAME}"
EOF

if [[ -f "${MOD_DIR}/manifest/mod_manifest.json" ]]; then
  cp -f "${MOD_DIR}/manifest/mod_manifest.json" "${PACK_PROJECT_DIR}/mod_manifest.json"
fi

rm -f "${EXPORT_ROOT}/mod_manifest.json"

if [[ -f "${PACK_SCRIPT_PATH}" ]]; then
  if GODOT_BIN="$(find_godot)"; then
    rm -f "${PCK_OUTPUT_PATH}"
    "${GODOT_BIN}" --headless --path "${PACK_PROJECT_DIR}" -s "${PACK_SCRIPT_PATH}" -- "${PCK_OUTPUT_PATH}" >/dev/null
    if [[ -f "${PCK_OUTPUT_PATH}" ]] && ! file "${PCK_OUTPUT_PATH}" | grep -q 'ASCII text'; then
      echo "生成 PCK：${PCK_OUTPUT_PATH}"
    else
      echo "PCK 生成失败或仍是占位文本：${PCK_OUTPUT_PATH}" >&2
      exit 1
    fi
  else
    echo "未找到 Godot，可继续使用现有 PCK 文件：${EXPORT_ROOT}/${MOD_NAME}.pck"
  fi
fi

echo "构建完成：${EXPORT_ROOT}/${MOD_NAME}.dll"
if [[ "${BUILD_MODE}" == "mcs" ]]; then
  echo "注意：当前 DLL 由 Mono 编译器生成，属于“真实程序集但目标框架仍待优化”的临时验证产物。"
fi
echo "注意：当前真实 PCK 只验证“有效资源包”这一个条件，不代表已符合 STS2 最终内容结构。"
