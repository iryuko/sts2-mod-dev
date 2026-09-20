using System.Reflection;
using System.Text.RegularExpressions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using Togawasakiko_in_Slay_the_Spire;

internal static class SongExpansionRegression
{
    private static readonly string[] Names = ["OctagramDance", "Divine", "InYourBlueEyes", "TheWholeBlueWorld"];

    private static void Equal<T>(T expected, T actual)
    {
        if (!Equals(expected, actual)) throw new Exception($"expected {expected}, got {actual}");
    }

    private static CardModel Card(CombatFixture f, string name, int player = 0)
    {
        if (typeof(TogawasakikoMod).Assembly.GetType("Togawasakiko_in_Slay_the_Spire." + name) == null)
            throw new Exception($"Approved Song {name} has not been implemented.");
        return f.Card(name, player);
    }

    private static string Description(CardModel card) => Regex.Replace(card.GetDescriptionForPile(PileType.Hand), @"\[[^\]]+\]", "");

    internal static async Task Run(Func<string, Func<Task>, Task> check)
    {
        TestLocalization.Load("eng");
        await check("New Songs keep dedicated album portraits through upgrade and downgrade", () =>
        {
            var f = new CombatFixture();
            string[] files = ["octagram_dance", "divine", "in_your_blue_eyes", "the_whole_blue_world"];
            for (int i = 0; i < Names.Length; i++)
            {
                CardModel card = Card(f, Names[i]);
                string path = $"res://mod_assets/cards/normal/uncommon/{files[i]}.png";
                Equal(path, card.PortraitPath);
                Equal(path, card.BetaPortraitPath);
                CardCmd.Upgrade(card, CardPreviewStyle.None);
                Equal(path, card.PortraitPath);
                CardCmd.Downgrade(card);
                Equal(path, card.PortraitPath);
            }
            return Task.CompletedTask;
        });

        await check("Octagram grants only its owner free ordinary cards, including late cards, and expires natively", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Togawasakiko");
            await CardCmd.AutoPlay(f.Choice, Card(f, "OctagramDance"), null);
            CardModel late = f.Card("TwoMoonsDeepIntoTheForest"), ally = f.Card("TwoMoonsDeepIntoTheForest", 1);
            await CardPileCmd.Add(late, PileType.Hand);
            await CardPileCmd.Add(ally, PileType.Hand);
            Equal(0, late.EnergyCost.GetWithModifiers(CostModifiers.All));
            Equal(7, ally.EnergyCost.GetWithModifiers(CostModifiers.All));
            Equal(0, (await late.SpendResources()).Item1);
            PowerModel power = f.Players[0].Creature.Powers.Single(p => p.Id.Entry == "OCTAGRAM_DANCE_POWER");
            await power.AfterSideTurnEnd(f.Choice, CombatSide.Player, [f.Players[1].Creature]);
            Equal(true, f.Players[0].Creature.Powers.Contains(power));
            await power.AfterSideTurnEnd(f.Choice, CombatSide.Player, [f.Players[0].Creature]);
            Equal(false, f.Players[0].Creature.Powers.Contains(power));
            Equal(6, late.EnergyCost.GetWithModifiers(CostModifiers.All));
            Equal(7, ally.EnergyCost.GetWithModifiers(CostModifiers.All));
        });

        foreach (string source in new[] { "OctagramDance", "TheWholeBlueWorld" })
        {
            await check($"Free X keeps current energy, star payment and native ResourceInfo: {source}", async () =>
            {
                var f = new CombatFixture("Togawasakiko", "Togawasakiko");
                CardModel x = f.Card("CrucifixX");
                CardModel card = Card(f, source);
                if (source == "TheWholeBlueWorld")
                {
                    CardCmd.Upgrade(card, CardPreviewStyle.None);
                    TestRngInjector.SetCombatCardGenerationOverride([x, f.Card("Ether")]);
                }
                await CardCmd.AutoPlay(f.Choice, card, null);
                await CardPileCmd.Add(x, PileType.Hand);
                await PlayerCmd.GainEnergy(3m, f.Players[0]);
                x.SetStarCostThisCombat(1);
                f.Players[0].PlayerCombatState!.GainStars(2);
                int energy = f.Players[0].PlayerCombatState!.Energy;
                (int spent, int stars) = await x.SpendResources();
                Equal(0, spent);
                Equal(1, stars);
                Equal(1, f.Players[0].PlayerCombatState!.Stars);
                Equal(energy, x.EnergyCost.CapturedXValue);
                Equal(energy, f.Players[0].PlayerCombatState!.Energy);
                await x.OnPlayWrapper(f.Choice, null, false, new ResourceInfo
                {
                    EnergySpent = spent, EnergyValue = spent, StarsSpent = stars, StarValue = stars
                });
                var play = CombatManager.Instance.History.CardPlaysFinished.Last(e => e.CardPlay.Card == x).CardPlay;
                Equal(energy, play.Resources.EnergyValue);
                Equal(0, play.Resources.EnergySpent);
                CardModel ordinaryX = f.Card("CrucifixX", 1);
                await CardPileCmd.Add(ordinaryX, PileType.Hand);
                await PlayerCmd.GainEnergy(2m, f.Players[1]);
                int otherEnergy = f.Players[1].PlayerCombatState!.Energy;
                Equal(otherEnergy, (await ordinaryX.SpendResources()).Item1);
                Equal(0, f.Players[1].PlayerCombatState!.Energy);
            });
        }

        await check("Blue World X clone retains combat-only free payment; a new combat and other copies do not", async () =>
        {
            var f = new CombatFixture();
            CardModel blue = Card(f, "TheWholeBlueWorld"), x = f.Card("CrucifixX");
            CardCmd.Upgrade(blue, CardPreviewStyle.None);
            TestRngInjector.SetCombatCardGenerationOverride([x, f.Card("Ether")]);
            await CardCmd.AutoPlay(f.Choice, blue, null);
            CardModel clone = x.CreateClone();
            await CardPileCmd.Add(clone, PileType.Hand);
            await PlayerCmd.GainEnergy(4m, f.Players[0]);
            Equal(0, (await clone.SpendResources()).Item1);
            Equal(4, clone.EnergyCost.CapturedXValue);
            clone.EnergyCost.EndOfTurnCleanup();
            Equal(0, (await clone.SpendResources()).Item1);
            CardModel normal = f.Card("CrucifixX");
            await CardPileCmd.Add(normal, PileType.Hand);
            Equal(4, (await normal.SpendResources()).Item1);
            f.StartCombat();
            await PlayerCmd.GainEnergy(3m, f.Players[0]);
            CardModel next = f.Card("CrucifixX");
            await CardPileCmd.Add(next, PileType.Hand);
            Equal(3, (await next.SpendResources()).Item1);
        });

        await check("Blue World relative reduction composes with Two Moons history instead of freezing it", async () =>
        {
            var f = new CombatFixture();
            CardModel moon = f.Card("TwoMoonsDeepIntoTheForest"), other = f.Card("TwoMoonsDeepIntoTheForest");
            await CardPileCmd.Add(other, PileType.Hand);
            TestRngInjector.SetCombatCardGenerationOverride([moon, f.Card("Ether")]);
            await CardCmd.AutoPlay(f.Choice, Card(f, "TheWholeBlueWorld"), null);
            Equal(5, CombatFixture.Cost(moon));
            Equal(6, CombatFixture.Cost(other));
            await f.Finish(f.Card("Ether"));
            Equal(4, CombatFixture.Cost(moon));
            Equal(5, CombatFixture.Cost(other));
        });

        await check("Blue World real RNG creates two new maintained Songs without touching the deck", async () =>
        {
            TestRngInjector.Cleanup();
            var f = new CombatFixture();
            CardModel blue = Card(f, "TheWholeBlueWorld");
            await CardPileCmd.Add(blue, PileType.Hand);
            var state = f.Players[0].PlayerCombatState!;
            var before = state.AllCards.ToHashSet();
            var deck = f.Players[0].Deck.Cards.ToArray();
            await CardCmd.AutoPlay(f.Choice, blue, null);
            var generated = state.AllCards.Where(card => !before.Contains(card)).ToArray();
            Equal(2, generated.Length);
            Type songType = typeof(TogawasakikoMod).Assembly.GetType("Togawasakiko_in_Slay_the_Spire.ISongCard", true)!;
            Equal(true, generated.All(card => songType.IsInstanceOfType(card) && card.DeckVersion == null && !card.IsUpgraded));
            Equal(true, deck.SequenceEqual(f.Players[0].Deck.Cards));
        });

        await check("Blue World full hand uses native overflow and retains the generated discount", async () =>
        {
            var f = new CombatFixture();
            for (int i = 0; i < 10; i++) await CardPileCmd.Add(f.Card("Slander"), PileType.Hand);
            CardModel first = f.Card("Ether"), second = f.Card("Face");
            TestRngInjector.SetCombatCardGenerationOverride([first, second]);
            CardModel blue = Card(f, "TheWholeBlueWorld");
            CardCmd.Upgrade(blue, CardPreviewStyle.None);
            await CardCmd.AutoPlay(f.Choice, blue, null);
            Equal(10, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
            Equal(PileType.Discard, first.Pile!.Type);
            Equal(PileType.Discard, second.Pile!.Type);
            Equal(0, CombatFixture.Cost(first));
            Equal(0, CombatFixture.Cost(second));
        });

        await check("DIVINE repeated play pays each time and does not reuse the first exhausted token", async () =>
        {
            var f = new CombatFixture();
            CardModel divine = Card(f, "Divine");
            divine.BaseReplayCount = 1;
            await CardPileCmd.Add(f.Card("OverworkAnxiety"), PileType.Hand);
            int energy = f.Players[0].PlayerCombatState!.Energy;
            await CardCmd.AutoPlay(f.Choice, divine, null);
            Equal(energy + 1, f.Players[0].PlayerCombatState!.Energy);
            Equal(2, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
            Equal(2, CombatManager.Instance.History.CardPlaysFinished.Count(e => e.CardPlay.Card == divine));
        });

        await check("Two Blue Eyes instances recurse only into inactive instances and finish native cleanup", async () =>
        {
            var f = new CombatFixture();
            CardModel first = Card(f, "InYourBlueEyes"), second = Card(f, "InYourBlueEyes");
            CardCmd.Upgrade(first, CardPreviewStyle.None);
            CardCmd.Upgrade(second, CardPreviewStyle.None);
            await CardPileCmd.Add(second, PileType.Exhaust);
            await CardCmd.AutoPlay(f.Choice, first, null);
            Equal(3, CombatManager.Instance.History.CardPlaysFinished.Count());
            Equal(PileType.Discard, first.Pile!.Type);
            Equal(PileType.Discard, second.Pile!.Type);
            Equal(0, f.Players[0].PlayerCombatState!.PlayPile.Cards.Count);
        });

        await check("Blue Eyes keeps its selected discard instance when the first Song reshuffles and draws it", async () =>
        {
            var f = new CombatFixture();
            foreach (CardModel card in f.Players[0].PlayerCombatState!.DrawPile.Cards.ToArray())
                await CardPileCmd.RemoveFromCombat(card);
            CardModel blue = Card(f, "InYourBlueEyes"), divine = Card(f, "Divine"), symbol = f.Card("SymbolIii");
            CardCmd.Upgrade(blue, CardPreviewStyle.None);
            await CardPileCmd.Add(divine, PileType.Exhaust);
            await CardPileCmd.Add(symbol, PileType.Discard);
            await CardPileCmd.Add(f.Card("OverworkAnxiety"), PileType.Hand);
            await CardCmd.AutoPlay(f.Choice, blue, null);
            Equal(2, CombatManager.Instance.History.CardPlaysFinished.Count(e => e.CardPlay.Card == divine));
            Equal(2, CombatManager.Instance.History.CardPlaysFinished.Count(e => e.CardPlay.Card == symbol));
            Equal(7, f.Players[0].Creature.Block);
            Equal(PileType.Discard, symbol.Pile!.Type);
        });

        foreach (bool upgraded in new[] { false, true })
        {
            await check($"Blue Eyes replays original growing Song with Dexterity twice; upgraded={upgraded}", async () =>
            {
                var f = new CombatFixture("Togawasakiko", "Togawasakiko");
                CardModel blue = Card(f, "InYourBlueEyes");
                if (upgraded) CardCmd.Upgrade(blue, CardPreviewStyle.None);
                CardModel song = f.Card("SymbolIii");
                CardCmd.Upgrade(song, CardPreviewStyle.None);
                CardModel discarded = f.Card("SymbolIii"), ally = f.Card("SymbolIii", 1);
                await CardPileCmd.Add(song, PileType.Exhaust);
                await CardPileCmd.Add(discarded, PileType.Discard);
                await CardPileCmd.Add(ally, PileType.Exhaust);
                ModelDb.Power<DexterityPower>().ToMutable().ApplyInternal(f.Players[0].Creature, 1, true);
                int energy = f.Players[0].PlayerCombatState!.Energy;
                await CardCmd.AutoPlay(f.Choice, blue, null);
                Equal(upgraded ? 26 : 17, f.Players[0].Creature.Block);
                Equal(2, CombatManager.Instance.History.CardPlaysFinished.Count(e => e.CardPlay.Card == song));
                Equal(upgraded ? 2 : 0, CombatManager.Instance.History.CardPlaysFinished.Count(e => e.CardPlay.Card == discarded));
                Equal(energy, f.Players[0].PlayerCombatState!.Energy);
                Equal(PileType.Exhaust, ally.Pile!.Type);
                Equal(PileType.Discard, song.Pile!.Type);
                Equal(0, song.BaseReplayCount);
                Equal(9m, song.DynamicVars.Block.BaseValue);
                await CardPileCmd.Add(song, PileType.Hand);
                song.UpdateDynamicVarPreview(CardPreviewMode.Normal, null, song.DynamicVars);
                Equal(true, Description(song).Contains("10"));
            });
        }

        foreach ((string name, PileType result) in new[] { ("Divine", PileType.Exhaust), ("AveMujica", PileType.None) })
        {
            await check($"Blue Eyes preserves native final pile for {name}", async () =>
            {
                var f = new CombatFixture();
                CardModel blue = Card(f, "InYourBlueEyes"), song = f.Card(name);
                await CardPileCmd.Add(song, PileType.Exhaust);
                await CardCmd.AutoPlay(f.Choice, blue, null);
                Equal(2, CombatManager.Instance.History.CardPlaysFinished.Count(e => e.CardPlay.Card == song));
                Equal(result, song.Pile?.Type ?? PileType.None);
            });
        }

        await check("Blue Eyes excludes unplayable and non-Song cards and takes no replacement snapshot", async () =>
        {
            var f = new CombatFixture();
            CardModel blue = Card(f, "InYourBlueEyes");
            CardCmd.Upgrade(blue, CardPreviewStyle.None);
            CardModel song = f.Card("SymbolIii"), unplayable = f.Card("ImprisonedXii"), nonSong = f.Card("Slander");
            foreach (CardModel card in new[] { song, unplayable, nonSong }) await CardPileCmd.Add(card, PileType.Exhaust);
            await CardCmd.AutoPlay(f.Choice, blue, null);
            Equal(2, CombatManager.Instance.History.CardPlaysFinished.Count(e => e.CardPlay.Card == song));
            Equal(PileType.Exhaust, unplayable.Pile!.Type);
            Equal(PileType.Exhaust, nonSong.Pile!.Type);
        });

        await check("Four new Songs are unique Uncommon Skill rewards in the maintained Song pool", () =>
        {
            var f = new CombatFixture();
            Type support = typeof(TogawasakikoMod).Assembly.GetType("Togawasakiko_in_Slay_the_Spire.ModSupport", true)!;
            var songs = (IReadOnlyList<CardModel>)support.GetMethod("GetSongPoolCanonicals")!.Invoke(null, null)!;
            var pool = CombatFixture.Canonical<CardPoolModel>("TogawasakikoCardPool").AllCards;
            foreach (string name in Names)
            {
                CardModel card = Card(f, name);
                Equal(1, songs.Count(c => c.Id == card.Id));
                Equal(1, pool.Count(c => c.Id == card.Id));
                Equal(CardRarity.Uncommon, card.Rarity);
                Equal(CardType.Skill, card.Type);
                Equal(true, File.Exists(Path.Combine(TestLocalization.RepoRoot,
                    "mods/Togawasakiko_in_Slay_the_Spire/pack", card.PortraitPath[6..])));
            }
            Equal(26, songs.Count);
            Equal(63, pool.Count());
            return Task.CompletedTask;
        });

        foreach (bool upgraded in new[] { false, true })
        {
            await check($"DIVINE costs zero and spends one token for energy and draw: {upgraded}", async () =>
            {
                var f = new CombatFixture();
                CardModel card = Card(f, "Divine");
                if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
                CardModel token = f.Card("OverworkAnxiety");
                await CardPileCmd.Add(token, PileType.Hand);
                var state = f.Players[0].PlayerCombatState!;
                int energy = state.Energy;
                await CardCmd.AutoPlay(f.Choice, card, null);
                Equal(0, CombatFixture.Cost(card));
                Equal(energy + 1, state.Energy);
                Equal(upgraded ? 3 : 2, state.Hand.Cards.Count);
                Equal(PileType.Exhaust, token.Pile!.Type);
                Equal(PileType.Exhaust, card.Pile!.Type);
            });

            await check($"Blue World loses five HP, generates two combat Songs with scoped costs: {upgraded}", async () =>
            {
                var f = new CombatFixture("Togawasakiko", "Togawasakiko");
                CardModel card = Card(f, "TheWholeBlueWorld", 1);
                if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
                var owner = f.Players[1];
                var deck = owner.Deck.Cards.ToArray();
                int hp = owner.Creature.CurrentHp;
                TestRngInjector.SetCombatCardGenerationOverride([f.Card("Ether", 1), f.Card("Face", 1)]);
                await CardCmd.AutoPlay(f.Choice, card, null);
                Equal(hp - 5, owner.Creature.CurrentHp);
                Equal(2, owner.PlayerCombatState!.Hand.Cards.Count);
                Equal(0, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
                Equal(true, deck.SequenceEqual(owner.Deck.Cards));
                Equal(1, CombatFixture.Cost(card));
                Equal(PileType.Discard, card.Pile!.Type);
                foreach (CardModel generated in owner.PlayerCombatState.Hand.Cards)
                {
                    Equal(owner, generated.Owner);
                    Equal(false, generated.IsUpgraded);
                    Equal(false, generated.IsCanonical);
                    Equal(null, generated.DeckVersion);
                    if (!generated.EnergyCost.CostsX)
                    {
                        int baseline = generated.CanonicalInstance.EnergyCost.Canonical;
                        Equal(upgraded ? 0 : Math.Max(0, baseline - 1), CombatFixture.Cost(generated));
                    }
                }
            });
        }

        await check("DIVINE grants neither energy nor draw without its own pressure token", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Togawasakiko");
            CardModel card = Card(f, "Divine");
            CardModel song = f.Card("Ether"), ally = f.Card("OverworkAnxiety", 1);
            await CardPileCmd.Add(song, PileType.Hand);
            await CardPileCmd.Add(ally, PileType.Hand);
            int energy = f.Players[0].PlayerCombatState!.Energy;
            await CardCmd.AutoPlay(f.Choice, card, null);
            Equal(energy, f.Players[0].PlayerCombatState!.Energy);
            Equal(1, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
            Equal(PileType.Hand, ally.Pile!.Type);
            Equal(PileType.Hand, song.Pile!.Type);
            Equal(PileType.Exhaust, card.Pile!.Type);
        });

        foreach (string language in new[] { "eng", "zhs" })
        {
            await check($"New Songs native upgrade/downgrade restores text, cost and keywords: {language}", () =>
            {
                TestLocalization.Load(language);
                var f = new CombatFixture();
                foreach ((string name, int cost, int upgradeCost) in new[] {
                    ("OctagramDance", 4, 3), ("Divine", 0, 0), ("InYourBlueEyes", 2, 2), ("TheWholeBlueWorld", 1, 1) })
                {
                    CardModel card = Card(f, name);
                    string before = Description(card);
                    Equal(false, before.Contains(".description"));
                    Equal(cost, CombatFixture.Cost(card));
                    CardCmd.Upgrade(card, CardPreviewStyle.None);
                    string after = Description(card);
                    Equal(upgradeCost, CombatFixture.Cost(card));
                    if (name != "OctagramDance") Equal(false, before == after);
                    Equal(name is "Divine" or "OctagramDance", card.Keywords.Contains(CardKeyword.Exhaust));
                    CardCmd.Downgrade(card);
                    Equal(before, Description(card));
                    Equal(cost, CombatFixture.Cost(card));
                    CardCmd.Upgrade(card, CardPreviewStyle.None);
                    Equal(after, Description(card));
                }
                return Task.CompletedTask;
            });
        }
        TestLocalization.Load("eng");
    }
}
