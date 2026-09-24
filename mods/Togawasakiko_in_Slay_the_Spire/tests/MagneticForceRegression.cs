using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;

internal static class MagneticForceRegression
{
    private const string PowerId = "MAGNETIC_FORCE_HELL_WARGOD_POWER";

    private static void Equal<T>(T expected, T actual)
    {
        if (!Equals(expected, actual)) throw new Exception($"expected {expected}, got {actual}");
    }

    internal static async Task Run(Func<string, Func<Task>, Task> check)
    {
        TestLocalization.Load("eng");

        await check("Magnetic Force reapplication neither multiplies nor consumes its turn-long replay", async () =>
        {
            var f = new CombatFixture();
            PowerModel power = await Activate(f);
            await Activate(f);
            Equal(1, f.Players[0].Creature.Powers.Count(p => p.Id.Entry == PowerId));
            // Single controls the power's presentation, not native amount accumulation.
            Equal(2, power.Amount);
            int hp = f.Enemy.CurrentHp;
            foreach (CardModel attack in new[] { f.Card("StrikeTogawasakiko"), f.Card("StrikeTogawasakiko") })
            {
                await CardCmd.AutoPlay(f.Choice, attack, f.Enemy, skipCardPileVisuals: true);
                Equal(2, FinishedPlays(attack).Length);
                Equal(2, power.Amount);
                Equal(true, f.Players[0].Creature.Powers.Contains(power));
            }
            Equal(hp - 24, f.Enemy.CurrentHp);
        });

        await check("Magnetic Force excludes another owner's attacks and its owner's skills and powers", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Togawasakiko");
            await Activate(f);
            CardModel allyAttack = f.Card("StrikeTogawasakiko", 1);
            int hp = f.Enemy.CurrentHp;
            await CardCmd.AutoPlay(f.Choice, allyAttack, f.Enemy, skipCardPileVisuals: true);
            Equal(1, FinishedPlays(allyAttack).Length);
            Equal(hp - 6, f.Enemy.CurrentHp);

            CardModel skill = f.Card("DefendTogawasakiko"), power = f.Card("Innocence");
            await CardCmd.AutoPlay(f.Choice, skill, null, skipCardPileVisuals: true);
            await CardCmd.AutoPlay(f.Choice, power, null, skipCardPileVisuals: true);
            Equal(1, FinishedPlays(skill).Length);
            Equal(5, f.Players[0].Creature.Block);
            Equal(1, FinishedPlays(power).Length);
            Equal(2, f.Players[0].Creature.Powers.Single(p => p.Id.Entry == "INNOCENCE_POWER").Amount);
        });

        await check("Magnetic Force expires on owner-side turn end and stops granting replays", async () =>
        {
            var f = new CombatFixture();
            PowerModel power = await Activate(f);
            await power.AfterSideTurnEnd(f.Choice, CombatSide.Enemy, [f.Enemy]);
            Equal(true, f.Players[0].Creature.Powers.Contains(power));
            await power.AfterSideTurnEnd(f.Choice, CombatSide.Player, [f.Players[0].Creature]);
            Equal(false, f.Players[0].Creature.Powers.Contains(power));
            CardModel attack = f.Card("StrikeTogawasakiko");
            await CardCmd.AutoPlay(f.Choice, attack, f.Enemy, skipCardPileVisuals: true);
            Equal(1, FinishedPlays(attack).Length);
            AssertCleanedUp(f, attack);
        });

        await check("Magnetic Force plus native Spiral produces exactly three plays and one wrapper completion", async () =>
        {
            var f = new CombatFixture();
            await Activate(f);
            CardModel attack = f.Card("StrikeTogawasakiko");
            CardCmd.Enchant<Spiral>(attack, 1m);
            int hp = f.Enemy.CurrentHp;
            int completed = 0;
            attack.Played += () => completed++;

            // Abort the old recursive implementation synchronously before it can hang the suite.
            void LimitReplay(AbstractModel _)
            {
                int started = CombatManager.Instance.History.CardPlaysStarted.Count(e => e.CardPlay.Card == attack);
                if (started > 3)
                    throw new Exception($"Magnetic Force exceeded the three-play Spiral bound: {started} plays started.");
            }

            attack.ExecutionFinished += LimitReplay;
            try
            {
                await CardCmd.AutoPlay(f.Choice, attack, f.Enemy, skipCardPileVisuals: true);
            }
            finally
            {
                attack.ExecutionFinished -= LimitReplay;
            }

            Equal(hp - 18, f.Enemy.CurrentHp);
            AssertNativeSeries(attack, 3, true);
            Equal(1, completed);
            Equal(PileType.Discard, attack.Pile!.Type);
            AssertCleanedUp(f, attack);
        });

        await check("Magnetic Force composes additively with One Two Punch without consuming itself", async () =>
        {
            var f = new CombatFixture();
            PowerModel magnetic = await Activate(f);
            await PowerCmd.Apply<OneTwoPunchPower>(f.Choice, f.Players[0].Creature, 1m, f.Players[0].Creature, null);
            CardModel attack = f.Card("StrikeTogawasakiko");
            int hp = f.Enemy.CurrentHp;
            await CardCmd.AutoPlay(f.Choice, attack, f.Enemy, skipCardPileVisuals: true);
            Equal(hp - 18, f.Enemy.CurrentHp);
            AssertNativeSeries(attack, 3, true);
            Equal(false, f.Players[0].Creature.Powers.OfType<OneTwoPunchPower>().Any());
            Equal(true, f.Players[0].Creature.Powers.Contains(magnetic));
            Equal(1, magnetic.Amount);
        });

        await check("Magnetic Force preserves paid X and star resources across both native plays", async () =>
        {
            var f = new CombatFixture();
            await Activate(f);
            CardModel attack = f.Card("CrucifixX");
            await CardPileCmd.Add(attack, PileType.Hand);
            await PlayerCmd.GainEnergy(3m, f.Players[0]);
            attack.SetStarCostThisCombat(1);
            f.Players[0].PlayerCombatState!.GainStars(2);
            int hp = f.Enemy.CurrentHp;
            (int energySpent, int starsSpent) = await attack.SpendResources();
            Equal(3, energySpent);
            Equal(1, starsSpent);
            Equal(3, attack.EnergyCost.CapturedXValue);
            Equal(0, f.Players[0].PlayerCombatState!.Energy);
            Equal(1, f.Players[0].PlayerCombatState!.Stars);

            await attack.OnPlayWrapper(f.Choice, null, false, new ResourceInfo
            {
                EnergySpent = energySpent, EnergyValue = energySpent,
                StarsSpent = starsSpent, StarValue = starsSpent
            }, skipCardPileVisuals: true);

            Equal(hp - 36, f.Enemy.CurrentHp);
            Equal(3, attack.EnergyCost.CapturedXValue);
            Equal(0, f.Players[0].PlayerCombatState!.Energy);
            Equal(1, f.Players[0].PlayerCombatState!.Stars);
            AssertNativeSeries(attack, 2, false);
            foreach (CardPlay play in FinishedPlays(attack))
            {
                Equal(3, play.Resources.EnergySpent);
                Equal(3, play.Resources.EnergyValue);
                Equal(1, play.Resources.StarsSpent);
                Equal(1, play.Resources.StarValue);
            }
            AssertCleanedUp(f, attack);
        });

        await check("Magnetic Force preserves native forced exhaustion and cleans the wrapper only once", async () =>
        {
            var f = new CombatFixture();
            await Activate(f);
            CardModel attack = f.Card("StrikeTogawasakiko");
            attack.EnergyCost.SetUntilPlayed(0);
            await CardPileCmd.Add(attack, PileType.Draw, CardPilePosition.Top);
            int completed = 0;
            attack.Played += () => completed++;

            // The native command sets ExhaustOnNextPlay before entering the wrapper.
            await CardPileCmd.AutoPlayFromDrawPile(f.Choice, f.Players[0], 1, CardPilePosition.Top, true);

            Equal(PileType.Exhaust, attack.Pile!.Type);
            Equal(false, attack.ExhaustOnNextPlay);
            Equal(false, attack.EnergyCost.HasLocalModifiers);
            Equal(1, completed);
            AssertNativeSeries(attack, 2, true);
            foreach (CardPlay play in FinishedPlays(attack)) Equal(PileType.Exhaust, play.ResultPile);
            AssertCleanedUp(f, attack);
        });

        foreach (bool magnetic in new[] { false, true })
        {
            string effect = magnetic ? "Magnetic Force" : "Native One Two Punch";
            await check($"{effect} retains a first-hit lethal target without retargeting the repeat", async () =>
            {
                var f = new CombatFixture();
                if (magnetic)
                    await Activate(f);
                else
                    await PowerCmd.Apply<OneTwoPunchPower>(f.Choice, f.Players[0].Creature, 1m, f.Players[0].Creature, null);
                var survivor = f.State.CreateCreature(ModelDb.Monster<TestSubject>().ToMutable(), CombatSide.Enemy, null);
                f.State.AddCreature(survivor);
                int survivorHp = survivor.CurrentHp;
                f.Enemy.SetCurrentHpInternal(6);
                CardModel attack = f.Card("StrikeTogawasakiko");
                int completed = 0;
                attack.Played += () => completed++;

                // Native count-based repeats retain CardPlay.Target; AttackCommand skips dead targets.
                await CardCmd.AutoPlay(f.Choice, attack, f.Enemy, skipCardPileVisuals: true);

                Equal(0, f.Enemy.CurrentHp);
                Equal(true, f.Enemy.IsDead);
                Equal(true, survivor.IsAlive);
                Equal(survivorHp, survivor.CurrentHp);
                Equal(true, CombatManager.Instance.IsInProgress);
                AssertNativeSeries(attack, 2, true);
                foreach (CardPlay play in FinishedPlays(attack)) Equal(f.Enemy, play.Target);
                Equal(1, completed);
                Equal(PileType.Discard, attack.Pile!.Type);
                AssertCleanedUp(f, attack);
            });
        }
    }

    private static async Task<PowerModel> Activate(CombatFixture f)
    {
        await f.PlayEffect(f.Card("TreasurePleasure"));
        return f.Players[0].Creature.Powers.Single(p => p.Id.Entry == PowerId);
    }

    private static CardPlay[] FinishedPlays(CardModel card) => CombatManager.Instance.History.CardPlaysFinished
        .Where(entry => entry.CardPlay.Card == card).Select(entry => entry.CardPlay).ToArray();

    private static void AssertNativeSeries(CardModel card, int count, bool autoPlay)
    {
        CardPlay[] plays = FinishedPlays(card);
        Equal(count, plays.Length);
        Equal(count, CombatManager.Instance.History.CardPlaysStarted.Count(entry => entry.CardPlay.Card == card));
        for (int i = 0; i < plays.Length; i++)
        {
            Equal(i, plays[i].PlayIndex);
            Equal(count, plays[i].PlayCount);
            Equal(autoPlay, plays[i].IsAutoPlay);
        }
    }

    private static void AssertCleanedUp(CombatFixture f, CardModel card)
    {
        Equal(0, f.Players[0].PlayerCombatState!.PlayPile.Cards.Count);
        Equal(false, CombatManager.Instance.IsExecutingCardOrPotionEffect(f.Players[0]));
        Equal(0, card.CurrentPlayIndex);
        Equal(null, card.CurrentTarget);
    }
}
