using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Audio;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(NAudioManager), nameof(NAudioManager.SetBgmVol))]
internal static class JukeboxBgmVolumePatch
{
    [HarmonyPrefix]
    private static void PreserveJukeboxMute(ref float volume)
    {
        volume = JukeboxOverlay.FilterBgmVolume(volume);
    }
}
