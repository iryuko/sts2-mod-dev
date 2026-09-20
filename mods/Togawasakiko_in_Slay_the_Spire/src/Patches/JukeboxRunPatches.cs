using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;

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

[HarmonyPatch(typeof(NRunMusicController), "_ExitTree")]
internal static class JukeboxRunMusicControllerExitPatch
{
    [HarmonyPostfix]
    private static void StopJukeboxPlayback()
    {
        if (RunManager.Instance.IsInProgress)
        {
            ModSupport.LogInfo("NRunMusicController exited while run is still in progress; preserving jukebox playback state.");
            return;
        }

        JukeboxRunInjector.StopPlaybackForRunExit();
    }
}

[HarmonyPatch(typeof(NRunMusicController), nameof(NRunMusicController.UpdateMusic), new Type[] { })]
internal static class JukeboxRunMusicControllerUpdateMusicPatch
{
    [HarmonyPostfix]
    private static void PreserveJukeboxPlayback()
    {
        JukeboxRunInjector.PreserveCustomPlaybackAfterRunMusicUpdate(nameof(NRunMusicController.UpdateMusic));
    }
}

[HarmonyPatch(typeof(NRunMusicController), nameof(NRunMusicController.UpdateTrack), new Type[] { })]
internal static class JukeboxRunMusicControllerUpdateTrackPatch
{
    [HarmonyPostfix]
    private static void PreserveJukeboxPlayback()
    {
        JukeboxRunInjector.PreserveCustomPlaybackAfterRunMusicUpdate(nameof(NRunMusicController.UpdateTrack));
    }
}

[HarmonyPatch(typeof(NRunMusicController), nameof(NRunMusicController.UpdateAmbience), new Type[] { })]
internal static class JukeboxRunMusicControllerUpdateAmbiencePatch
{
    [HarmonyPostfix]
    private static void PreserveJukeboxPlayback()
    {
        JukeboxRunInjector.PreserveCustomPlaybackAfterRunMusicUpdate(nameof(NRunMusicController.UpdateAmbience));
    }
}
