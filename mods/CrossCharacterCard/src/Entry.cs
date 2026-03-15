using System;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;

namespace CrossCharacterCard
{
    [ModInitializer("Initialize")]
    public static class CrossCharacterCardMod
    {
        private static readonly CardPoolRule[] Rules =
        {
            new CardPoolRule(
                "BodySlam -> SilentCardPool",
                static () => ModHelper.AddModelToPool<SilentCardPool, BodySlam>())
        };

        public static void Initialize()
        {
            Console.WriteLine($"[CrossCharacterCard] Initialize start. Registering {Rules.Length} card-pool rule(s).");

            foreach (CardPoolRule rule in Rules)
            {
                rule.Apply();
                Console.WriteLine($"[CrossCharacterCard] Registered rule: {rule.Description}");
            }
        }

        private sealed class CardPoolRule
        {
            private readonly Action _apply;

            public CardPoolRule(string description, Action apply)
            {
                Description = description;
                _apply = apply;
            }

            public string Description { get; }

            public void Apply()
            {
                _apply();
            }
        }
    }
}
