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


class MultiplayerContextContracts(unittest.TestCase):
    def test_run_migrations_cover_all_players(self) -> None:
        entry = read_source("src/Entry.cs")
        block = entry[
            entry.index("private static void OnRunStarted"):
            entry.index("private static ShadowOfThePastCard")
        ]
        self.assertIn("foreach (Player player in runState.Players)", block)
        self.assertNotIn("GetLocalPlayer(runState)", block)

    def test_hook_task_is_registered(self) -> None:
        support = read_source("src/ModSupport.cs")
        cards = read_source("src/Cards/TogawasakikoSongCards.cs")
        self.assertIn("public static async Task RunCombatHookTask(", support)
        self.assertIn("AssignTaskAndWaitForPauseOrCompletion(task)", support)
        self.assertIn("await ModSupport.RunCombatHookTask(", cards)

    def test_detached_gameplay_context_is_absent(self) -> None:
        support = read_source("src/ModSupport.cs")
        self.assertNotIn("CreateDetachedChoiceContext", support)
        self.assertNotIn("CreateBestEffortCombatChoiceContext", support)
        self.assertNotIn(
            "public static async Task ApplyPressure(Creature target",
            support,
        )

    def test_kill_kiss_uses_throwing_context(self) -> None:
        powers = read_source("src/Powers/TogawasakikoSongPowers.cs")
        self.assertIn(
            "PlayerChoiceContext choiceContext = new ThrowingPlayerChoiceContext();",
            powers,
        )

    def test_current_v0107_public_apis_are_called_directly(self) -> None:
        support = read_source("src/ModSupport.cs")
        for old_resolver in (
            "ResolveCreatureCombatStateProperty",
            "ResolvePowerApplyCreatureMethod",
            "ResolvePowerModifyAmountMethod",
            "ResolveAddGeneratedCardToCombatMethod",
            "ResolveAttackTargetingAllOpponentsMethod",
            "ResolveHookPlayerChoiceContextModelConstructor",
        ):
            self.assertNotIn(old_resolver, support)
        self.assertIn("creature?.CombatState as CombatState", support)
        self.assertIn("PowerCmd.Apply<T>(choiceContext", support)
        self.assertIn("PowerCmd.ModifyAmount(choiceContext", support)
        self.assertIn("CardPileCmd.AddGeneratedCardToCombat(", support)
        self.assertIn("command.TargetingAllOpponents(combatState)", support)


class GameplayStateContracts(unittest.TestCase):
    def test_watcher_invalid_state_is_not_swallowed(self) -> None:
        patch = read_source("src/Patches/CombatWatcherPatches.cs")
        self.assertNotIn("catch (Exception ex)", patch)
        self.assertIn("Expected exactly one combat watcher", patch)

    def test_pressure_redemption_has_one_owner(self) -> None:
        powers = read_source("src/Powers/TogawasakikoPowers.cs")
        self.assertIn("IsPressureRedemptionOwner", powers)
        self.assertIn("cardSource?.Owner?.Character is Togawasakiko", powers)
        self.assertIn("applier?.Player?.Character is Togawasakiko", powers)

    def test_magnetic_replay_uses_native_play_count(self) -> None:
        powers = read_source("src/Powers/TogawasakikoPowers.cs")
        magnetic = powers.split("internal sealed class MagneticForceHellWargodPower", 1)[1]
        magnetic = magnetic.split("internal sealed class SocialWithdrawalPower", 1)[0]
        self.assertIn("override int ModifyCardPlayCount(", magnetic)
        self.assertNotIn("CardCmd.AutoPlay", magnetic)
        self.assertNotIn("HashSet<CardModel>", magnetic)

    def test_relic_cards_use_original_command_chain(self) -> None:
        relics = read_source("src/Relics/BestCompanion.cs")
        self.assertEqual(2, relics.count("Owner.RunState.CreateCard("))
        self.assertEqual(2, relics.count("CardPileCmd.Add(card, PileType.Deck)"))
        self.assertEqual(2, relics.count("CardCmd.PreviewCardPileAdd"))
        self.assertNotIn("AddSpecificCardToDeck", relics)


class PrivateReflectionContracts(unittest.TestCase):
    def test_optional_ui_has_no_static_field_ref(self) -> None:
        for relative in (
            "src/Patches/CardLibraryPatches.cs",
            "src/Patches/TogawaEventRoomPatches.cs",
        ):
            source = read_source(relative)
            self.assertNotIn("FieldRefAccess", source, relative)
            self.assertNotIn("AccessTools.FieldRef<", source, relative)

    def test_teiji_does_not_mutate_run_state(self) -> None:
        source = read_source("src/Patches/TogawaEventRoomPatches.cs")
        self.assertNotIn("PlayerRunStateSetter", source)
        self.assertNotIn("PropertySetter(typeof(Player)", source)
        self.assertNotIn("RunStateRef(__instance) =", source)
        self.assertNotIn("class TogawaEventRoomSetupPatch", source)

    def test_event_fallback_uses_public_layout(self) -> None:
        source = read_source("src/Patches/TogawaEventRoomPatches.cs")
        self.assertIn("Lazy<FieldInfo?>", source)
        self.assertIn("__instance.Layout", source)
        self.assertIn("return true;", source)
