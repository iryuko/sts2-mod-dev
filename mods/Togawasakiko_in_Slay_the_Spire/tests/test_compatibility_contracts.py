from pathlib import Path
import unittest
import xml.etree.ElementTree as ET


MOD_ROOT = Path(__file__).resolve().parents[1]
REPO_ROOT = Path(__file__).resolve().parents[3]
SRC_ROOT = MOD_ROOT / "src"


def read_source(relative_path: str) -> str:
    return (MOD_ROOT / relative_path).read_text(encoding="utf-8")


class ReferenceConfigurationContracts(unittest.TestCase):
    def test_csproj_exposes_reference_overrides(self) -> None:
        root = ET.parse(
            SRC_ROOT / "Togawasakiko_in_Slay_the_Spire.csproj"
        ).getroot()
        values = {node.tag: (node.text or "").strip() for node in root.iter()}
        self.assertEqual(
            "../../../references/game-dlls/sts2/arm64",
            values["Sts2ReferenceDir"],
        )
        self.assertEqual(
            "../../../references/game-dlls/shared/0Harmony.dll",
            values["HarmonyReferencePath"],
        )
        hints = [node.text.strip() for node in root.iter("HintPath") if node.text]
        self.assertIn("$(Sts2ReferenceDir)/sts2.dll", hints)
        self.assertIn("$(Sts2ReferenceDir)/GodotSharp.dll", hints)
        self.assertIn("$(HarmonyReferencePath)", hints)

    def test_dual_reference_verifier_exists(self) -> None:
        script = MOD_ROOT / "scripts" / "verify-api-compatibility.py"
        self.assertTrue(script.is_file())
        text = script.read_text(encoding="utf-8")
        self.assertIn("--mac-reference-dir", text)
        self.assertIn("--windows-reference-dir", text)
        self.assertIn("--no-incremental", text)


class DarvOriginalFlowContracts(unittest.TestCase):
    def test_mod_does_not_patch_darv(self) -> None:
        self.assertFalse((SRC_ROOT / "Patches" / "DarvPatches.cs").exists())

    def test_curseslander_is_ancient(self) -> None:
        cards = read_source("src/Cards/TogawasakikoCards.cs")
        character = read_source("src/Characters/Togawasakiko.cs")
        start = cards.index("internal sealed class Curseslander")
        end = cards.index("internal sealed class Unendurable", start)
        block = cards[start:end]
        self.assertIn("CardRarity.Ancient", block)
        self.assertIn('GetAncientPortraitPath("curseslander.png")', block)
        self.assertIn("ModelDb.Card<Curseslander>()", character)
