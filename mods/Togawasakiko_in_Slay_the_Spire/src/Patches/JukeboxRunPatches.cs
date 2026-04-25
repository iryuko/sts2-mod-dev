using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(NRun), "_Ready")]
internal static class JukeboxRunReadyPatch
{
    [HarmonyPostfix]
    private static void RegisterRunAndInject(NRun __instance)
    {
        JukeboxRunInjector.RegisterRun(__instance);
    }
}

[HarmonyPatch(typeof(NGlobalUi), "_Ready")]
internal static class JukeboxGlobalUiReadyPatch
{
    [HarmonyPostfix]
    private static void InjectOverlay(NGlobalUi __instance)
    {
        JukeboxRunInjector.TryInjectIntoGlobalUi(__instance);
    }
}
