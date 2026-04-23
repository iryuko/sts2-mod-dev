using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(CardPoolModel), "get_EnergyIconPath")]
internal static class EnergyIconPatches
{
    private const string TogawasakikoCardEnergyIconPath = "res://images/ui/card/energy_togawasakiko.png";

    [HarmonyPrefix]
    private static bool UseCustomEnergyIcon(CardPoolModel __instance, ref string __result)
    {
        if (__instance is not TogawasakikoCardPool)
        {
            return true;
        }

        __result = TogawasakikoCardEnergyIconPath;
        return false;
    }
}

[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Helpers.EnergyIconHelper), nameof(MegaCrit.Sts2.Core.Helpers.EnergyIconHelper.GetPath), new[] { typeof(string) })]
internal static class EnergyIconHelperPatches
{
    private const string TogawasakikoInlineEnergyIconPath = "res://images/packed/sprite_fonts/togawasakiko_energy_icon.png";

    [HarmonyPrefix]
    private static bool UseCustomEnergyIcon(string prefix, ref string __result)
    {
        if (!string.Equals(prefix, "togawasakiko", System.StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        __result = TogawasakikoInlineEnergyIconPath;
        return false;
    }
}
