using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(AncientEventModel), "GenerateInitialOptionsWrapper")]
internal static class TogawaAncientConsoleOptionDedupPatch
{
    [HarmonyPostfix]
    private static void ReorderDebugOptionWithoutDroppingChoices(
        AncientEventModel __instance,
        ref IReadOnlyList<EventOption> __result)
    {
        if (__instance is not TogawaTeiji || string.IsNullOrEmpty(__instance.DebugOption) || __result.Count <= 1)
        {
            return;
        }

        List<EventOption> allPossible = new(__instance.AllPossibleOptions);
        EventOption? selectedOption = allPossible.Find(option =>
            !string.IsNullOrEmpty(option.TextKey)
            && option.TextKey.Contains(__instance.DebugOption, StringComparison.Ordinal));
        if (selectedOption == null)
        {
            return;
        }

        List<EventOption> ordered = new(allPossible.Count) { selectedOption };
        foreach (EventOption option in allPossible)
        {
            if (!string.Equals(option.TextKey, selectedOption.TextKey, StringComparison.Ordinal))
            {
                ordered.Add(option);
            }
        }

        __result = ordered;

        AccessTools.Field(typeof(AncientEventModel), "_generatedOptions")?.SetValue(__instance, ordered);
    }
}
