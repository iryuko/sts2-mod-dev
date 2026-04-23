using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllCharacters), MethodType.Getter)]
internal static class CharacterModelPatches
{
    [HarmonyPostfix]
    private static IEnumerable<CharacterModel> AppendTogawasakiko(IEnumerable<CharacterModel> __result)
    {
        CharacterModel togawasakiko = ModelDb.Character<Togawasakiko>();
        if (__result.Any(existing => existing.Id == togawasakiko.Id))
        {
            return __result;
        }

        return __result.Concat(new[] { togawasakiko });
    }
}

[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.MerchantAnimPath), MethodType.Getter)]
internal static class CharacterModelMerchantAnimPathPatch
{
    private const string TogawasakikoMerchantAnimPath = "res://scenes/merchant/characters/togawasakiko_merchant.tscn";

    [HarmonyPostfix]
    private static void UseTogawasakikoMerchantScene(CharacterModel __instance, ref string __result)
    {
        if (__instance is Togawasakiko)
        {
            __result = TogawasakikoMerchantAnimPath;
        }
    }
}

[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.RestSiteAnimPath), MethodType.Getter)]
internal static class CharacterModelRestSiteAnimPathPatch
{
    private const string TogawasakikoRestSiteAnimPath = "res://scenes/rest_site/characters/togawasakiko_rest_site.tscn";

    [HarmonyPostfix]
    private static void UseTogawasakikoRestSiteScene(CharacterModel __instance, ref string __result)
    {
        if (__instance is Togawasakiko)
        {
            __result = TogawasakikoRestSiteAnimPath;
        }
    }
}

[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.EnergyCounterPath), MethodType.Getter)]
internal static class CharacterModelEnergyCounterPathPatch
{
    private const string TogawasakikoEnergyCounterPath = "res://scenes/combat/energy_counters/togawasakiko_energy_counter.tscn";

    [HarmonyPostfix]
    private static void UseTogawasakikoEnergyCounter(CharacterModel __instance, ref string __result)
    {
        if (__instance is Togawasakiko)
        {
            __result = TogawasakikoEnergyCounterPath;
        }
    }
}
