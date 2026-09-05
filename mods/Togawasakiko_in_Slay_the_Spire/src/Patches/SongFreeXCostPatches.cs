using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch]
internal static class SongFreeXCostPatches
{
    private static readonly Func<CardModel, int, Task> SpendStars =
        AccessTools.MethodDelegate<Func<CardModel, int, Task>>(AccessTools.Method(typeof(CardModel), "SpendStars"));

    internal static bool IsFreeX(CardModel card)
    {
        if (card.IsCanonical || !card.EnergyCost.CostsX || card.CombatState == null
            || card.Owner?.PlayerCombatState == null)
        {
            return false;
        }

        return ModSupport.GetPower<OctagramDancePower>(card.Owner.Creature)?.GrantsFreeEnergy == true
            || (card is TogawasakikoCard song && song.BlueWorldFreeXCombat == card.CombatState
                && ModSupport.HasCombatZeroCostModifier(card));
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.SpendResources))]
    [HarmonyPrefix]
    private static bool SpendResources(CardModel __instance, ref Task<(int, int)> __result)
    {
        if (!IsFreeX(__instance))
        {
            return true;
        }

        __result = SpendFreeX(__instance);
        return false;
    }

    private static async Task<(int, int)> SpendFreeX(CardModel card)
    {
        int stars = Math.Max(0, card.GetStarCostWithModifiers());
        card.EnergyCost.CapturedXValue = card.Owner.PlayerCombatState!.Energy;
        // SpendEnergy(0) would erase X. Keep the native zero-spend hook and star payment.
        await Hook.AfterEnergySpent(card.CombatState!, card, 0);
        await SpendStars(card, stars);
        return (0, stars);
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    [HarmonyPrefix]
    private static void PreserveXValue(CardModel __instance, bool isAutoPlay, ref ResourceInfo resources)
    {
        if (!isAutoPlay && resources.EnergySpent == 0 && IsFreeX(__instance))
        {
            resources = new ResourceInfo
            {
                EnergySpent = 0,
                EnergyValue = __instance.EnergyCost.CapturedXValue,
                StarsSpent = resources.StarsSpent,
                StarValue = resources.StarValue
            };
        }
    }
}
