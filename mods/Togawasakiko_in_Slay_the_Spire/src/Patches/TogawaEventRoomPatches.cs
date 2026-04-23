using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(NEventRoom), "SetupLayout")]
internal static class TogawaEventRoomSetupPatch
{
    private static readonly AccessTools.FieldRef<NEventRoom, EventModel?> EventRef =
        AccessTools.FieldRefAccess<NEventRoom, EventModel?>("_event");

    private static readonly AccessTools.FieldRef<NEventRoom, IRunState?> RunStateRef =
        AccessTools.FieldRefAccess<NEventRoom, IRunState?>("_runState");

    private static readonly MethodInfo? PlayerRunStateSetter =
        AccessTools.PropertySetter(typeof(Player), nameof(Player.RunState));

    [HarmonyPrefix]
    private static void EnsureTogawaRunState(NEventRoom __instance)
    {
        try
        {
            EventModel? eventModel = EventRef(__instance);
            if (eventModel is not TogawaTeiji)
            {
                return;
            }

            Player? owner = eventModel.Owner;
            IRunState? roomRunState = RunStateRef(__instance);
            IRunState? ownerRunState = owner?.RunState;
            IRunState? managerRunState = AccessTools.Property(typeof(RunManager), "State")?.GetValue(RunManager.Instance) as IRunState;
            IRunState? resolvedRunState = roomRunState ?? ownerRunState ?? managerRunState;

            if (resolvedRunState != null)
            {
                if (roomRunState == null)
                {
                    RunStateRef(__instance) = resolvedRunState;
                }

                if (owner != null && ownerRunState == null)
                {
                    PlayerRunStateSetter?.Invoke(owner, new object?[] { resolvedRunState });
                }
            }

            string ownerCharacter = owner?.Character?.Id?.Entry ?? "null";
            int resolvedPlayers = resolvedRunState?.Players?.Count ?? -1;
            int resolvedActs = resolvedRunState?.Acts?.Count ?? -1;
            ModSupport.LogInfo(
                $"Togawa SetupLayout preflight owner={(owner != null)} ownerCharacter={ownerCharacter} " +
                $"ownerRunState={(owner?.RunState != null)} roomRunState={(RunStateRef(__instance) != null)} " +
                $"managerRunState={(managerRunState != null)} players={resolvedPlayers} acts={resolvedActs}");
        }
        catch (Exception ex)
        {
            ModSupport.LogWarn($"Togawa SetupLayout preflight failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(NEventRoom), "SetDescription")]
internal static class TogawaEventRoomDescriptionPatch
{
    private static readonly AccessTools.FieldRef<NEventRoom, EventModel?> EventRef =
        AccessTools.FieldRefAccess<NEventRoom, EventModel?>("_event");

    private static readonly AccessTools.FieldRef<NEventRoom, IRunState?> RunStateRef =
        AccessTools.FieldRefAccess<NEventRoom, IRunState?>("_runState");

    private static readonly MethodInfo? GetLayoutMethod =
        AccessTools.Method(typeof(NEventRoom), "get_Layout");

    private static readonly MethodInfo? PlayerRunStateSetter =
        AccessTools.PropertySetter(typeof(Player), nameof(Player.RunState));

    [HarmonyPrefix]
    private static bool SetTogawaDescriptionSafely(NEventRoom __instance, LocString description)
    {
        EventModel? eventModel = EventRef(__instance);
        if (eventModel is not TogawaTeiji)
        {
            return true;
        }

        if (!description.Exists())
        {
            ModSupport.LogWarn("Togawa SetDescription received missing LocString; falling back to original NEventRoom.SetDescription.");
            return true;
        }

        try
        {
            Player? owner = eventModel.Owner;
            IRunState? resolvedRunState = owner?.RunState ?? RunStateRef(__instance);
            if (resolvedRunState == null)
            {
                resolvedRunState = AccessTools.Property(typeof(RunManager), "State")?.GetValue(RunManager.Instance) as IRunState;
            }

            if (owner != null && owner.RunState == null && resolvedRunState != null)
            {
                PlayerRunStateSetter?.Invoke(owner, new object?[] { resolvedRunState });
            }

            CharacterModel? character = owner?.Character;
            if (character != null)
            {
                character.AddDetailsTo(description);
            }

            bool isMultiplayer = (owner?.RunState?.Players?.Count ?? resolvedRunState?.Players?.Count ?? 1) > 1;
            description.Add("IsMultiplayer", isMultiplayer);
            eventModel.DynamicVars?.AddTo(description);

            if (GetLayoutMethod?.Invoke(__instance, null) is NEventLayout layout)
            {
                layout.SetDescription(description.GetFormattedText());
            }

            return false;
        }
        catch (Exception ex)
        {
            ModSupport.LogWarn($"Togawa SetDescription fallback hit: {ex}");

            try
            {
                if (GetLayoutMethod?.Invoke(__instance, null) is NEventLayout layout)
                {
                    layout.SetDescription(description.GetFormattedText());
                    return false;
                }
            }
            catch (Exception inner)
            {
                ModSupport.LogWarn($"Togawa SetDescription hard fallback failed: {inner}");
            }

            return true;
        }
    }
}
