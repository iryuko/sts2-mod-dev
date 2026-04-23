using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Runs;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllAncients), MethodType.Getter)]
internal static class TogawaAncientModelDbPatch
{
    [HarmonyPostfix]
    private static IEnumerable<AncientEventModel> AppendTogawaTeiji(IEnumerable<AncientEventModel> __result)
    {
        return TogawaAncientPatchHelpers.AppendTogawaTeiji(__result);
    }
}

// Glory is the current Act 3 pool; Togawa Teiji should only be eligible there.
[HarmonyPatch(typeof(Glory), "get_AllAncients")]
internal static class TogawaAncientAct3AllAncientsPatch
{
    [HarmonyPostfix]
    private static IEnumerable<AncientEventModel> AppendTogawaTeiji(IEnumerable<AncientEventModel> __result)
    {
        return TogawaAncientPatchHelpers.AppendTogawaTeiji(__result);
    }
}

[HarmonyPatch(typeof(Glory), nameof(Glory.GetUnlockedAncients))]
internal static class TogawaAncientAct3UnlockedAncientsPatch
{
    [HarmonyPostfix]
    private static IEnumerable<AncientEventModel> AppendTogawaTeiji(IEnumerable<AncientEventModel> __result)
    {
        return TogawaAncientPatchHelpers.AppendTogawaTeiji(__result);
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.ShouldAllowAncient))]
internal static class TogawaAncientAvailabilityPatch
{
    [HarmonyPostfix]
    private static void RestrictTogawaTeiji(
        IRunState runState,
        Player player,
        AncientEventModel ancient,
        ref bool __result)
    {
        if (ancient is TogawaTeiji)
        {
            __result = __result && player?.Character is Togawasakiko;
        }
    }
}

internal static class TogawaAncientPatchHelpers
{
    public static IEnumerable<AncientEventModel> AppendTogawaTeiji(IEnumerable<AncientEventModel> ancients)
    {
        AncientEventModel togawaTeiji = ModelDb.AncientEvent<TogawaTeiji>();
        if (ancients.Any(existing => existing.Id == togawaTeiji.Id))
        {
            return ancients;
        }

        return ancients.Concat(new[] { togawaTeiji });
    }
}
