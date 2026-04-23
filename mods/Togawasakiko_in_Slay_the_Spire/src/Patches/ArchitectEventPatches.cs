using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Events;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(TheArchitect), "get_DialogueSet")]
internal static class ArchitectEventDialogueSetPatch
{
    private static bool _loggedSilentAlias;

    [HarmonyPostfix]
    private static void EnsureTogawasakikoArchitectDialogue(TheArchitect __instance, AncientDialogueSet? __result)
    {
        if (__result?.CharacterDialogues == null)
        {
            return;
        }

        if (__instance.Owner?.Character is not Togawasakiko)
        {
            return;
        }

        Dictionary<string, IReadOnlyList<AncientDialogue>> dialogues = __result.CharacterDialogues;
        string togawasakikoKey = ModelDb.Character<Togawasakiko>().Id.Entry;
        if (dialogues.ContainsKey(togawasakikoKey))
        {
            return;
        }

        string silentKey = ModelDb.Character<Silent>().Id.Entry;
        if (!dialogues.TryGetValue(silentKey, out IReadOnlyList<AncientDialogue>? silentDialogues))
        {
            ModSupport.LogWarn("Architect dialogue set is missing the Silent entry; could not alias Togawasakiko.");
            return;
        }

        dialogues[togawasakikoKey] = silentDialogues;
        if (_loggedSilentAlias)
        {
            return;
        }

        _loggedSilentAlias = true;
        ModSupport.LogInfo(
            $"Aliased Architect dialogue for {togawasakikoKey} to {silentKey} so the base-game Architect attack cadence can run.");
    }
}
