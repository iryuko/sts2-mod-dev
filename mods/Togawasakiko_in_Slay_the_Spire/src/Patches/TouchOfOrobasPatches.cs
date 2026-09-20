using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(TouchOfOrobas), nameof(TouchOfOrobas.GetUpgradedStarterRelic))]
internal static class TouchOfOrobasDollMaskUpgradePatch
{
    private static void Postfix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic is DollMask)
        {
            __result = ModelDb.Relic<PianoOfMom>().ToMutable();
        }
    }
}
