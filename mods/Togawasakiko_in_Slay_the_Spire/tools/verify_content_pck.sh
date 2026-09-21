#!/usr/bin/env bash
set -euo pipefail
if [[ $# -lt 5 || $# -gt 6 ]]; then
  echo 'Usage: verify_content_pck.sh <godot> <qa-project> <pck> <spine-sha256> <log-dir> [resource-hashes.json]' >&2
  exit 2
fi
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GODOT_BIN="$1"
QA_PROJECT="$2"
PCK_PATH="$3"
SPINE_SHA="$4"
LOG_DIR="$5"
mkdir -p "$LOG_DIR"
RESOURCE_ARGS=()
if [[ $# -eq 6 ]]; then RESOURCE_ARGS+=("$6"); fi
run_check() {
  local script="$1" marker="$2" log="$3"
  shift 3
  if ! "$GODOT_BIN" --path "$QA_PROJECT" --script "$SCRIPT_DIR/$script" --fixed-fps 600 -- "$@" > "$log" 2>&1; then
    cat "$log"
    return 1
  fi
  if rg -n '^ERROR:|SCRIPT ERROR:' "$log"; then return 1; fi
  if ! rg -Fq "$marker" "$log"; then
    cat "$log"
    echo "Missing completion marker: $marker" >&2
    return 1
  fi
}
run_check verify_content_pck.gd CONTENT_PCK_PASS: "$LOG_DIR/pck-verification.log" "$PCK_PATH" "$SPINE_SHA" "${RESOURCE_ARGS[@]}"
run_check verify_thorn_render.gd 'THORN_RENDER_SWEEP_COMPLETE=1181 scales=3' "$LOG_DIR/thorn-render-sweep.log" "$PCK_PATH"
echo 'CONTENT_AND_RENDER_PCK_PASS: resource checks and 3543 boundary samples, zero logged errors'
