using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.SetUpCombat))]
internal static class CombatWatcherPatches
{
    private static void Postfix(CombatState state)
    {
        foreach (Player player in state.Players.Where(player => player.Character is Togawasakiko))
        {
            InstallForPlayer(state, player);
        }

        foreach (Player player in state.Players.Where(player => player.Character is Togawasakiko))
        {
            int watcherCount = player.Creature?.Powers.OfType<TogawasakikoCombatWatcherPower>().Count() ?? 0;
            if (watcherCount != 1)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one combat watcher for player={player.NetId}, found {watcherCount}.");
            }
        }
    }

    private static void InstallForPlayer(CombatState state, Player player)
    {
        Creature? creature = player.Creature;
        if (creature == null || ModSupport.GetCombatState(creature) != state)
        {
            throw new InvalidOperationException(
                $"Cannot install combat watcher for player={player.NetId}: creature is not attached to this combat.");
        }

        List<TogawasakikoCombatWatcherPower> watchers = creature.Powers
            .OfType<TogawasakikoCombatWatcherPower>()
            .ToList();
        if (watchers.Count > 1)
        {
            throw new InvalidOperationException(
                $"Expected exactly one combat watcher for player={player.NetId}, found {watchers.Count} before install.");
        }

        TogawasakikoCombatWatcherPower? watcher = watchers.SingleOrDefault();
        if (watcher == null)
        {
            watcher = (TogawasakikoCombatWatcherPower)ModelDb.Power<TogawasakikoCombatWatcherPower>().ToMutable();
            watcher.ApplyInternal(creature, 1m, silent: true);
            ModSupport.LogInfo($"Installed combat watcher during SetUpCombat for player={player.NetId}.");
        }
        else
        {
            ModSupport.LogInfo($"Reset existing combat watcher during SetUpCombat for player={player.NetId}.");
        }

        watcher.ResetCombatState();
        ModSupport.ClearPersistedTwoMoonsCostModifiers(player);
    }
}
