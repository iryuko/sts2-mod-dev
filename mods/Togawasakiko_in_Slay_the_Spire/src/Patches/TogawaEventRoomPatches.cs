using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(NEventRoom), "SetDescription")]
internal static class TogawaEventRoomDescriptionPatch
{
    private static readonly Lazy<FieldInfo?> EventField =
        new(ResolveEventField);

    [HarmonyPrefix]
    private static bool SetTogawaDescriptionSafely(NEventRoom __instance, LocString description)
    {
        try
        {
            FieldInfo? eventField = EventField.Value;
            if (eventField == null || eventField.GetValue(__instance) is not TogawaTeiji eventModel)
            {
                return true;
            }

            if (!description.Exists())
            {
                ModSupport.LogWarn(
                    "Togawa SetDescription received missing LocString; falling back to original NEventRoom.SetDescription.");
                return true;
            }

            Player? owner = eventModel.Owner;
            NEventLayout? layout = __instance.Layout;
            if (owner == null || layout == null)
            {
                return true;
            }

            CharacterModel? character = owner.Character;
            if (character != null)
            {
                character.AddDetailsTo(description);
            }

            bool isMultiplayer = (owner.RunState?.Players.Count ?? 1) > 1;
            description.Add("IsMultiplayer", isMultiplayer);
            eventModel.DynamicVars?.AddTo(description);
            layout.SetDescription(description.GetFormattedText());
            return false;
        }
        catch (Exception ex)
        {
            ModSupport.LogWarn($"Togawa SetDescription integration failed; using original flow: {ex}");
            return true;
        }
    }

    private static FieldInfo? ResolveEventField()
    {
        FieldInfo? field = AccessTools.Field(typeof(NEventRoom), "_event");
        if (field == null)
        {
            ModSupport.LogWarn("Togawa event description integration disabled: missing NEventRoom._event.");
        }

        return field;
    }
}
