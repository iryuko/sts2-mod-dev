using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(NAncientDialogueLine), nameof(NAncientDialogueLine.Create))]
internal static class AncientDialogueLineFallbackPatch
{
    private const string AncientDialogueLineScenePath = "res://scenes/events/ancient_dialogue_line.tscn";

    [HarmonyPrefix]
    private static bool LoadDialogueLineWithoutCacheWhenNeeded(
        AncientDialogueLine line,
        AncientEventModel ancient,
        CharacterModel character,
        ref NAncientDialogueLine __result)
    {
        if (ancient is not TogawaTeiji)
        {
            return true;
        }

        if (PreloadManager.Cache.GetScene(AncientDialogueLineScenePath) != null)
        {
            return true;
        }

        PackedScene? scene = ResourceLoader.Load<PackedScene>(AncientDialogueLineScenePath);
        if (scene == null)
        {
            return true;
        }

        NAncientDialogueLine dialogueLine = scene.Instantiate<NAncientDialogueLine>();
        AccessTools.Field(typeof(NAncientDialogueLine), "_line")?.SetValue(dialogueLine, line);
        AccessTools.Field(typeof(NAncientDialogueLine), "_ancient")?.SetValue(dialogueLine, ancient);
        AccessTools.Field(typeof(NAncientDialogueLine), "_character")?.SetValue(dialogueLine, character);
        __result = dialogueLine;
        return false;
    }
}
