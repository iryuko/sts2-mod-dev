using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllRelicPools), MethodType.Getter)]
internal static class TogawasakikoAllRelicPoolsPatch
{
    [HarmonyPostfix]
    private static IEnumerable<RelicPoolModel> AppendTogawaRelicPools(IEnumerable<RelicPoolModel> __result)
    {
        return RelicPoolPatchHelpers.AppendTogawaRelicPools(__result);
    }
}

internal static class RelicPoolPatchHelpers
{
    public static IEnumerable<RelicPoolModel> AppendTogawaRelicPools(IEnumerable<RelicPoolModel> relicPools)
    {
        List<RelicPoolModel> appendedPools = relicPools.ToList();
        RelicPoolModel ancientPool = ModelDb.RelicPool<TogawasakikoAncientRelicPool>();
        if (!appendedPools.Any(existing => existing.Id == ancientPool.Id))
        {
            appendedPools.Add(ancientPool);
        }

        RelicPoolModel specialPool = ModelDb.RelicPool<TogawasakikoSpecialRelicPool>();
        if (!appendedPools.Any(existing => existing.Id == specialPool.Id))
        {
            appendedPools.Add(specialPool);
        }

        return appendedPools;
    }
}
