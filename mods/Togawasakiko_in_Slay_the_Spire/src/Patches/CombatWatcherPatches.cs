using System;
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
        try
        {
            foreach (Player player in state.Players.Where(player => player.Character is Togawasakiko))
            {
                InstallForPlayer(state, player);
            }
        }
        catch (Exception ex)
        {
            ModSupport.LogError("Failed while installing combat watcher after combat setup: " + ex);
        }
    }

    private static void InstallForPlayer(CombatState state, Player player)
    {
        Creature? creature = player.Creature;
        if (creature == null || creature.CombatState != state)
        {
            ModSupport.LogWarn($"Skipped combat watcher install for player={player.NetId}: creature is not attached to this combat.");
            return;
        }

        TogawasakikoCombatWatcherPower? watcher = creature.Powers
            .OfType<TogawasakikoCombatWatcherPower>()
            .FirstOrDefault();

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
