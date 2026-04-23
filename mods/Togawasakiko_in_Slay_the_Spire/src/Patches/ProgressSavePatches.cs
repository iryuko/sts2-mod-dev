using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Saves.Managers;

namespace Togawasakiko_in_Slay_the_Spire;

internal static class ProgressSavePatchHelpers
{
    public static bool ShouldRunCharacterEpochCheck(Player? localPlayer)
    {
        CharacterModel? character = localPlayer?.Character;
        if (character == null)
        {
            return true;
        }

        if (ModSupport.IsBaseGameCharacter(character))
        {
            return true;
        }

        ModSupport.LogInfo($"Skipping base-game-only epoch progress check for custom character {character.Id.Entry}.");
        return false;
    }
}

[HarmonyPatch(typeof(ProgressSaveManager), "CheckFifteenElitesDefeatedEpoch")]
internal static class ProgressSaveManagerCheckFifteenElitesDefeatedEpochPatch
{
    [HarmonyPrefix]
    private static bool SkipUnsupportedCharacter(Player localPlayer)
    {
        return ProgressSavePatchHelpers.ShouldRunCharacterEpochCheck(localPlayer);
    }
}

[HarmonyPatch(typeof(ProgressSaveManager), "CheckFifteenBossesDefeatedEpoch")]
internal static class ProgressSaveManagerCheckFifteenBossesDefeatedEpochPatch
{
    [HarmonyPrefix]
    private static bool SkipUnsupportedCharacter(Player localPlayer)
    {
        return ProgressSavePatchHelpers.ShouldRunCharacterEpochCheck(localPlayer);
    }
}

[HarmonyPatch(typeof(ProgressSaveManager), "ObtainCharUnlockEpoch")]
internal static class ProgressSaveManagerObtainCharUnlockEpochPatch
{
    [HarmonyPrefix]
    private static bool SkipUnsupportedCharacter(Player localPlayer, int act)
    {
        if (ProgressSavePatchHelpers.ShouldRunCharacterEpochCheck(localPlayer))
        {
            return true;
        }

        CharacterModel? character = localPlayer?.Character;
        if (character != null)
        {
            ModSupport.LogInfo(
                $"Skipping base-game-only character unlock epoch lookup for custom character {character.Id.Entry} after Act {act + 1}.");
        }

        return false;
    }
}
