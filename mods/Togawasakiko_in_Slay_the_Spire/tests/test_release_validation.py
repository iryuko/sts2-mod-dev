import hashlib
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
import zipfile


MOD_ROOT = Path(__file__).resolve().parents[1]
MOD_NAME = "Togawasakiko_in_Slay_the_Spire"
VALIDATOR = MOD_ROOT / "scripts" / "validate-release-staging.py"


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


class ReleaseValidationTests(unittest.TestCase):
    def setUp(self) -> None:
        self.temporary_directory = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary_directory.name)
        self.stage_root = self.root / "stage"
        self.mod_dir = self.stage_root / MOD_NAME
        self.mod_dir.mkdir(parents=True)

        source_manifest = json.loads(
            (MOD_ROOT / "manifest" / "mod_manifest.json").read_text(encoding="utf-8")
        )
        self.version = str(source_manifest["version"])
        self.manifest_bytes = json.dumps(
            source_manifest,
            ensure_ascii=True,
            indent=2,
        ).encode("utf-8")

        (self.mod_dir / f"{MOD_NAME}.dll").write_bytes(b"test-dll")
        (self.mod_dir / f"{MOD_NAME}.pck").write_bytes(b"test-pck")
        (self.mod_dir / "mod_manifest.json").write_bytes(self.manifest_bytes)

        self.zip_path = self.root / f"{MOD_NAME}-v{self.version}.zip"
        self.hash_path = self.root / f"{MOD_NAME}-v{self.version}.sha256"
        self.write_zip(MOD_NAME)
        self.write_hash_file()

    def tearDown(self) -> None:
        self.temporary_directory.cleanup()

    def write_zip(self, top_level: str) -> None:
        with zipfile.ZipFile(self.zip_path, "w", zipfile.ZIP_DEFLATED) as archive:
            for path in sorted(self.mod_dir.iterdir()):
                archive.write(path, f"{top_level}/{path.name}")

    def write_hash_file(self) -> None:
        staged_files = [
            self.mod_dir / f"{MOD_NAME}.dll",
            self.mod_dir / f"{MOD_NAME}.pck",
            self.mod_dir / "mod_manifest.json",
        ]
        records = [(self.zip_path.name, sha256(self.zip_path))]
        records.extend(
            (path.relative_to(self.stage_root).as_posix(), sha256(path))
            for path in staged_files
        )
        self.hash_path.write_text(
            "".join(f"{digest}  {name}\n" for name, digest in records),
            encoding="utf-8",
        )

    def run_validator(self, expected_version: str | None = None) -> subprocess.CompletedProcess[str]:
        return subprocess.run(
            [
                sys.executable,
                str(VALIDATOR),
                "--expected-version",
                expected_version or self.version,
                "--staging-root",
                str(self.stage_root),
                "--zip",
                str(self.zip_path),
                "--sha256-file",
                str(self.hash_path),
            ],
            check=False,
            capture_output=True,
            text=True,
        )

    def test_valid_staging_passes(self) -> None:
        result = self.run_validator()
        self.assertEqual(0, result.returncode, result.stderr)
        self.assertIn(f"version={self.version}", result.stdout)

    def test_expected_version_mismatch_fails(self) -> None:
        result = self.run_validator("0.2.2")
        self.assertEqual(1, result.returncode)
        self.assertIn("expected version 0.2.2", result.stderr)

    def test_tampered_artifact_fails_hash_validation(self) -> None:
        lines = self.hash_path.read_text(encoding="utf-8").splitlines()
        lines[1] = f"{'0' * 64}  {MOD_NAME}/{MOD_NAME}.dll"
        self.hash_path.write_text("\n".join(lines) + "\n", encoding="utf-8")

        result = self.run_validator()
        self.assertEqual(1, result.returncode)
        self.assertIn("SHA-256 mismatch", result.stderr)

    def test_extra_staged_file_fails_allowlist(self) -> None:
        (self.mod_dir / "debug.log").write_text("unexpected", encoding="utf-8")

        result = self.run_validator()
        self.assertEqual(1, result.returncode)
        self.assertIn("staging allowlist mismatch", result.stderr)

    def test_zip_with_wrong_top_level_fails(self) -> None:
        self.write_zip("wrong-mod-name")
        self.write_hash_file()

        result = self.run_validator()
        self.assertEqual(1, result.returncode)
        self.assertIn("ZIP members mismatch", result.stderr)


if __name__ == "__main__":
    unittest.main()
