#!/usr/bin/env python3

import argparse
import hashlib
import json
from pathlib import Path
import re
import sys
from typing import Mapping
import zipfile


MOD_NAME = "Togawasakiko_in_Slay_the_Spire"
MIN_GAME_VERSION = "0.107.1"
INSTALL_FILES = {
    "Togawasakiko_in_Slay_the_Spire.dll",
    "Togawasakiko_in_Slay_the_Spire.pck",
    "mod_manifest.json",
}
MOD_ROOT = Path(__file__).resolve().parents[1]


def load_manifest(path: Path) -> dict[str, object]:
    data = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(data, dict):
        raise ValueError(f"manifest must contain a JSON object: {path}")
    return data


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def parse_hash_file(path: Path) -> dict[str, str]:
    records: dict[str, str] = {}
    for line_number, line in enumerate(path.read_text(encoding="utf-8").splitlines(), 1):
        match = re.fullmatch(r"([0-9a-fA-F]{64})  (\S+)", line)
        if match is None:
            raise ValueError(f"invalid SHA-256 record at {path}:{line_number}")
        digest, name = match.groups()
        if name in records:
            raise ValueError(f"duplicate SHA-256 record: {name}")
        records[name] = digest.lower()
    return records


def validate_manifest_identity(
    expected: str,
    source: Mapping[str, object],
    pack: Mapping[str, object],
    staged: Mapping[str, object],
) -> None:
    manifests = {"source": source, "pack": pack, "staged": staged}
    versions = {name: manifest.get("version") for name, manifest in manifests.items()}
    if any(version != expected for version in versions.values()):
        raise ValueError(f"expected version {expected}; manifest versions are {versions}")

    game_versions = {
        name: manifest.get("min_game_version")
        for name, manifest in manifests.items()
    }
    if any(version != MIN_GAME_VERSION for version in game_versions.values()):
        raise ValueError(
            f"expected min_game_version {MIN_GAME_VERSION}; manifest values are {game_versions}"
        )


def validate_staging_allowlist(mod_dir: Path) -> None:
    if not mod_dir.is_dir():
        raise ValueError(f"staged mod directory is missing: {mod_dir}")

    entries = {path.name for path in mod_dir.iterdir()}
    if entries != INSTALL_FILES:
        raise ValueError(
            f"staging allowlist mismatch: expected {sorted(INSTALL_FILES)}, got {sorted(entries)}"
        )
    non_files = sorted(path.name for path in mod_dir.iterdir() if not path.is_file())
    if non_files:
        raise ValueError(f"staging entries must be files: {non_files}")


def validate_zip(
    zip_path: Path,
    mod_name: str,
    staged_files: Mapping[str, Path],
) -> None:
    expected_members = {f"{mod_name}/{name}" for name in staged_files}
    with zipfile.ZipFile(zip_path) as archive:
        members = archive.namelist()
        if len(members) != len(expected_members) or set(members) != expected_members:
            raise ValueError(
                f"ZIP members mismatch: expected {sorted(expected_members)}, got {sorted(members)}"
            )

        for name, staged_path in staged_files.items():
            member = f"{mod_name}/{name}"
            if archive.read(member) != staged_path.read_bytes():
                raise ValueError(f"ZIP member bytes differ from staging: {member}")


def validate_hashes(
    records: Mapping[str, str],
    zip_path: Path,
    mod_dir: Path,
) -> None:
    paths = {zip_path.name: zip_path}
    paths.update(
        {
            path.relative_to(mod_dir.parent).as_posix(): path
            for path in sorted(mod_dir.iterdir())
        }
    )
    if list(records) != list(paths):
        raise ValueError(
            f"SHA-256 record names mismatch: expected {sorted(paths)}, got {sorted(records)}"
        )

    for name, path in paths.items():
        actual = sha256(path)
        if records[name] != actual:
            raise ValueError(
                f"SHA-256 mismatch for {name}: expected {records[name]}, actual {actual}"
            )


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Validate a Togawasakiko release staging directory.")
    parser.add_argument("--expected-version", required=True)
    parser.add_argument("--staging-root", type=Path, required=True)
    parser.add_argument("--zip", dest="zip_path", type=Path, required=True)
    parser.add_argument("--sha256-file", type=Path, required=True)
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    mod_dir = args.staging_root / MOD_NAME
    staged_files = {name: mod_dir / name for name in INSTALL_FILES}
    source_manifest_path = MOD_ROOT / "manifest" / "mod_manifest.json"
    pack_manifest_path = MOD_ROOT / "pack" / "mod_manifest.json"
    staged_manifest_path = mod_dir / "mod_manifest.json"

    try:
        source_manifest = load_manifest(source_manifest_path)
        pack_manifest = load_manifest(pack_manifest_path)
        staged_manifest = load_manifest(staged_manifest_path)
        validate_manifest_identity(
            args.expected_version,
            source_manifest,
            pack_manifest,
            staged_manifest,
        )
        validate_staging_allowlist(mod_dir)
        validate_zip(args.zip_path, MOD_NAME, staged_files)
        records = parse_hash_file(args.sha256_file)
        validate_hashes(records, args.zip_path, mod_dir)
    except (OSError, ValueError, zipfile.BadZipFile) as error:
        print(f"release validation failed: {error}", file=sys.stderr)
        return 1

    print(f"release staging valid: version={args.expected_version}")
    print(f"staging={mod_dir}")
    print(f"zip={args.zip_path}")
    print(f"sha256={args.sha256_file}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
