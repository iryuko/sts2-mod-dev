using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(CardFactory), nameof(CardFactory.CreateForMerchant), new[] { typeof(Player), typeof(IEnumerable<CardModel>), typeof(CardType) })]
internal static class CardFactoryMerchantFallbackPatch
{
    private const string MissingMerchantRarityMessage = "Can't generate a valid rarity for the merchant card options passed.";

    [HarmonyFinalizer]
    private static Exception? RecoverInvalidMerchantRarity(
        Player player,
        IEnumerable<CardModel> options,
        CardType type,
        ref CardCreationResult __result,
        Exception? __exception)
    {
        if (__exception is not InvalidOperationException invalidOperation
            || player.Character is not Togawasakiko
            || !string.Equals(invalidOperation.Message, MissingMerchantRarityMessage, StringComparison.Ordinal))
        {
            return __exception;
        }

        List<CardModel> typedCandidates = options
            .Where(card => card.Type == type && Togawasakiko.IsRewardEligibleCard(card))
            .Distinct()
            .ToList();

        foreach (CardRarity fallbackRarity in new[] { CardRarity.Uncommon, CardRarity.Common, CardRarity.Rare })
        {
            List<CardModel> rarityCandidates = typedCandidates
                .Where(card => card.Rarity == fallbackRarity)
                .ToList();

            if (rarityCandidates.Count == 0)
            {
                continue;
            }

            ModSupport.LogWarn(
                $"Recovered merchant rarity generation for type={type} by falling back to {fallbackRarity} with [{string.Join(", ", rarityCandidates.Select(card => card.Id.Entry))}].");
            __result = CardFactory.CreateForMerchant(player, rarityCandidates, fallbackRarity);
            return null;
        }

        return __exception;
    }
}
