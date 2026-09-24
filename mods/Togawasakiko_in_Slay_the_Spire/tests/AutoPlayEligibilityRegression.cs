using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

internal static class AutoPlayEligibilityRegression
{
    internal static async Task Run(Func<string, Func<Task>, Task> check)
    {
        await check("Ave Mujica autoplays an unaffordable card without spending energy", async () =>
        {
            var f = new CombatFixture();
            var player = f.Players[0];
            await PlayerCmd.SetEnergy(3, player);
            CardModel card = f.Card("DefendTogawasakiko");
            card.EnergyCost.SetThisCombat(4);
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top);
            var power = CombatFixture.Canonical<PowerModel>("AveMujicaPower").ToMutable();
            power.ApplyInternal(player.Creature, 1, true);
            await power.AfterPlayerTurnStartLate(f.Choice, player);
            if (card.Pile?.Type != PileType.Discard || player.Creature.Block != 5
                || player.PlayerCombatState!.Energy != 3
                || CombatManager.Instance.History.CardPlaysFinished.Count(e => e.CardPlay.Card == card) != 1)
                throw new Exception($"Free autoplay failed: pile={card.Pile?.Type}, block={player.Creature.Block}, energy={player.PlayerCombatState!.Energy}.");
        });

        await check("Ave Mujica still draws an Unplayable card instead of auto-discarding it", async () =>
        {
            var f = new CombatFixture();
            CardModel card = f.State.CreateCard(ModelDb.Card<Dazed>(), f.Players[0]);
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top);
            var power = CombatFixture.Canonical<PowerModel>("AveMujicaPower").ToMutable();
            power.ApplyInternal(f.Players[0].Creature, 1, true);
            await power.AfterPlayerTurnStartLate(f.Choice, f.Players[0]);
            if (card.Pile?.Type != PileType.Hand
                || CombatManager.Instance.History.CardPlaysFinished.Any(e => e.CardPlay.Card == card))
                throw new Exception("The unplayable card was not drawn without playing.");
        });

        await check("Ave Mujica ignores star-only unaffordability without spending either resource", async () =>
        {
            var f = new CombatFixture();
            var player = f.Players[0];
            await PlayerCmd.SetEnergy(3, player);
            player.PlayerCombatState!.GainStars(1);
            CardModel card = f.Card("DefendTogawasakiko");
            card.SetStarCostThisCombat(2);
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top);
            if (card.CanPlay(out UnplayableReason reason, out _) || reason != UnplayableReason.StarCostTooHigh)
                throw new Exception($"Expected star-only unaffordability, got {reason}.");

            var power = CombatFixture.Canonical<PowerModel>("AveMujicaPower").ToMutable();
            power.ApplyInternal(player.Creature, 1, true);
            await power.AfterPlayerTurnStartLate(f.Choice, player);
            CardPlay[] plays = CombatManager.Instance.History.CardPlaysFinished
                .Where(entry => entry.CardPlay.Card == card).Select(entry => entry.CardPlay).ToArray();
            if (card.Pile?.Type != PileType.Discard || player.Creature.Block != 5
                || player.PlayerCombatState.Energy != 3 || player.PlayerCombatState.Stars != 1
                || plays.Length != 1 || !plays[0].IsAutoPlay
                || plays[0].Resources.EnergySpent != 0 || plays[0].Resources.StarsSpent != 0)
                throw new Exception($"Star-only free autoplay failed: pile={card.Pile?.Type}, block={player.Creature.Block}, energy={player.PlayerCombatState.Energy}, stars={player.PlayerCombatState.Stars}, plays={plays.Length}.");
        });

        await check("Ave Mujica preserves genuine restrictions combined with energy and star failures", async () =>
        {
            var f = new CombatFixture();
            var player = f.Players[0];
            await PlayerCmd.SetEnergy(3, player);
            player.PlayerCombatState!.GainStars(1);
            CardModel card = f.Card("DefendTogawasakiko");
            card.EnergyCost.SetThisCombat(4);
            card.SetStarCostThisCombat(2);
            CardCmd.ApplyKeyword(card, CardKeyword.Unplayable);
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top);
            const UnplayableReason expected = UnplayableReason.EnergyCostTooHigh
                | UnplayableReason.StarCostTooHigh | UnplayableReason.HasUnplayableKeyword;
            if (card.CanPlay(out UnplayableReason reason, out _) || reason != expected)
                throw new Exception($"Expected mixed affordability and Unplayable reasons, got {reason}.");

            var power = CombatFixture.Canonical<PowerModel>("AveMujicaPower").ToMutable();
            power.ApplyInternal(player.Creature, 1, true);
            await power.AfterPlayerTurnStartLate(f.Choice, player);
            if (card.Pile?.Type != PileType.Hand || player.Creature.Block != 0
                || player.PlayerCombatState.Energy != 3 || player.PlayerCombatState.Stars != 1
                || CombatManager.Instance.History.CardPlaysStarted.Any(entry => entry.CardPlay.Card == card)
                || CombatManager.Instance.History.CardPlaysFinished.Any(entry => entry.CardPlay.Card == card))
                throw new Exception("The resource mask bypassed a genuine Unplayable restriction.");
        });
    }
}
