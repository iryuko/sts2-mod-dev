using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllEvents), MethodType.Getter)]
internal static class UnattendedPianoModelDbEventsPatch
{
    [HarmonyPostfix]
    private static IEnumerable<EventModel> AppendUnattendedPiano(IEnumerable<EventModel> __result)
    {
        return QuestionRoomEventPatchHelpers.AppendUnattendedPiano(__result);
    }
}

[HarmonyPatch(typeof(Glory), "get_AllEvents")]
internal static class UnattendedPianoGloryEventsPatch
{
    [HarmonyPostfix]
    private static IEnumerable<EventModel> AppendUnattendedPiano(IEnumerable<EventModel> __result)
    {
        return QuestionRoomEventPatchHelpers.AppendUnattendedPiano(__result);
    }
}

[HarmonyPatch(typeof(Hive), "get_AllEvents")]
internal static class UnattendedPianoHiveEventsPatch
{
    [HarmonyPostfix]
    private static IEnumerable<EventModel> AppendUnattendedPiano(IEnumerable<EventModel> __result)
    {
        return QuestionRoomEventPatchHelpers.AppendUnattendedPiano(__result);
    }
}

[HarmonyPatch(typeof(Overgrowth), "get_AllEvents")]
internal static class UnattendedPianoOvergrowthEventsPatch
{
    [HarmonyPostfix]
    private static IEnumerable<EventModel> AppendUnattendedPiano(IEnumerable<EventModel> __result)
    {
        return QuestionRoomEventPatchHelpers.AppendUnattendedPiano(__result);
    }
}

[HarmonyPatch(typeof(Underdocks), "get_AllEvents")]
internal static class UnattendedPianoUnderdocksEventsPatch
{
    [HarmonyPostfix]
    private static IEnumerable<EventModel> AppendUnattendedPiano(IEnumerable<EventModel> __result)
    {
        return QuestionRoomEventPatchHelpers.AppendUnattendedPiano(__result);
    }
}

internal static class QuestionRoomEventPatchHelpers
{
    public static IEnumerable<EventModel> AppendUnattendedPiano(IEnumerable<EventModel> events)
    {
        EventModel unattendedPiano = ModelDb.Event<UnattendedPiano>();
        if (events.Any(existing => existing.Id == unattendedPiano.Id))
        {
            return events;
        }

        return events.Concat([unattendedPiano]);
    }
}
