#!/usr/bin/env python3

import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys


MOD_ROOT = Path(__file__).resolve().parents[1]
REPO_ROOT = MOD_ROOT.parents[1]
PROJECT = MOD_ROOT / "src" / "Togawasakiko_in_Slay_the_Spire.csproj"
MAC_HARMONY = REPO_ROOT / "references" / "game-dlls" / "shared" / "0Harmony.dll"
EXPECTED_GAME_VERSION = "v0.107.1"
EXPECTED_GAME_COMMIT = "59260271"


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Build Togawasakiko against retained Mac and Windows STS2 APIs."
    )
    parser.add_argument("--mac-reference-dir", required=True, type=Path)
    parser.add_argument("--windows-reference-dir", required=True, type=Path)
    parser.add_argument(
        "--output-root",
        type=Path,
        default=REPO_ROOT / "local" / "api-compatibility-builds" / "togawasakiko",
    )
    return parser.parse_args()


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def require_reference_set(reference_dir: Path, harmony_path: Path) -> None:
    required = [
        reference_dir / "sts2.dll",
        reference_dir / "GodotSharp.dll",
        harmony_path,
    ]
    missing = [str(path) for path in required if not path.is_file()]
    if missing:
        raise FileNotFoundError("Missing API reference files: " + ", ".join(missing))


def validate_windows_identity(reference_dir: Path) -> dict[str, object]:
    release_info_path = reference_dir / "release_info.json"
    if not release_info_path.is_file():
        raise FileNotFoundError(f"Missing Windows release identity: {release_info_path}")

    release_info = json.loads(release_info_path.read_text(encoding="utf-8"))
    version = release_info.get("version")
    commit = release_info.get("commit")
    if version != EXPECTED_GAME_VERSION or commit != EXPECTED_GAME_COMMIT:
        raise ValueError(
            "Windows API identity mismatch: "
            f"expected {EXPECTED_GAME_VERSION}/{EXPECTED_GAME_COMMIT}, "
            f"got {version}/{commit}."
        )
    return release_info


def print_reference_evidence(
    label: str,
    reference_dir: Path,
    harmony_path: Path,
) -> None:
    print(f"[{label}] reference_dir={reference_dir}")
    for path in (
        reference_dir / "sts2.dll",
        reference_dir / "GodotSharp.dll",
        harmony_path,
    ):
        print(f"[{label}] sha256 {path.name} {sha256(path)}")


def build(
    label: str,
    reference_dir: Path,
    harmony_path: Path,
    output_root: Path,
) -> None:
    output_dir = output_root / label
    output_dir.mkdir(parents=True, exist_ok=True)
    command = [
        "dotnet",
        "build",
        str(PROJECT),
        "-c",
        "Release",
        "--no-incremental",
        f"-p:Sts2ReferenceDir={reference_dir.resolve()}",
        f"-p:HarmonyReferencePath={harmony_path.resolve()}",
        f"-p:OutputPath={output_dir.resolve()}",
    ]
    print(f"[{label}] command={' '.join(command)}")
    subprocess.run(command, cwd=REPO_ROOT, check=True)


def main() -> int:
    args = parse_args()
    mac_reference_dir = args.mac_reference_dir.resolve()
    windows_reference_dir = args.windows_reference_dir.resolve()
    output_root = args.output_root.resolve()
    windows_harmony = windows_reference_dir / "0Harmony.dll"

    try:
        require_reference_set(mac_reference_dir, MAC_HARMONY)
        require_reference_set(windows_reference_dir, windows_harmony)
        windows_identity = validate_windows_identity(windows_reference_dir)
        print(
            "[windows] identity "
            f"version={windows_identity['version']} "
            f"commit={windows_identity['commit']}"
        )
        print("[mac] identity=retained-v0.107.1-reference-set")
        print_reference_evidence("mac", mac_reference_dir, MAC_HARMONY)
        print_reference_evidence("windows", windows_reference_dir, windows_harmony)
        build("mac", mac_reference_dir, MAC_HARMONY, output_root)
        build("windows", windows_reference_dir, windows_harmony, output_root)
    except (FileNotFoundError, ValueError, subprocess.CalledProcessError) as error:
        print(str(error), file=sys.stderr)
        return 1

    print(f"API compatibility builds completed: {output_root}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
