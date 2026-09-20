using System;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace SilentBonusRelic
{
    [ModInitializer("Initialize")]
    public static class SilentBonusRelicMod
    {
        private static readonly StartingRelicRule[] Rules =
        {
            new StartingRelicRule(
                "SneckoSkull",
                static player => player.Relics.Any(relic => relic is SneckoSkull),
                static () => ModelDb.Relic<SneckoSkull>().ToMutable()),
            new StartingRelicRule(
                "Shuriken",
                static player => player.Relics.Any(relic => relic is Shuriken),
                static () => ModelDb.Relic<Shuriken>().ToMutable())
        };

        public static void Initialize()
        {
            Console.WriteLine("[SilentBonusRelic] Initialize start.");
            RunManager.Instance.RunStarted += OnRunStarted;
            Console.WriteLine($"[SilentBonusRelic] Registered RunStarted handler with {Rules.Length} starting-relic rule(s).");
        }

        private static void OnRunStarted(RunState runState)
        {
            try
            {
                TryGrantSneckoSkullAtRunStart(runState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SilentBonusRelic] RunStarted handler failed: {ex}");
            }
        }

        private static void TryGrantSneckoSkullAtRunStart(RunState runState)
        {
            Player? player = runState.Players.FirstOrDefault();
            if (player == null)
            {
                Console.WriteLine("[SilentBonusRelic] No player available on RunStarted.");
                return;
            }

            if (player.Character is not Silent)
            {
                Console.WriteLine($"[SilentBonusRelic] Current run character is not Silent: {player.Character.GetType().Name}");
                return;
            }

            foreach (StartingRelicRule rule in Rules)
            {
                if (rule.AlreadyHas(player))
                {
                    Console.WriteLine($"[SilentBonusRelic] Silent already has {rule.Description}.");
                    continue;
                }

                RelicModel relic = rule.CreateRelic();
                relic.FloorAddedToDeck = 1;
                SaveManager.Instance.MarkRelicAsSeen(relic);
                player.AddRelicInternal(relic, -1, true);
                Console.WriteLine($"[SilentBonusRelic] Added {rule.Description} to Silent on RunStarted.");
            }
        }

        private sealed class StartingRelicRule
        {
            public StartingRelicRule(string description, Func<Player, bool> alreadyHas, Func<RelicModel> createRelic)
            {
                Description = description;
                AlreadyHas = alreadyHas;
                CreateRelic = createRelic;
            }

            public string Description { get; }

            public Func<Player, bool> AlreadyHas { get; }

            public Func<RelicModel> CreateRelic { get; }
        }
    }
}
