using System.Reflection;
using System.Text.RegularExpressions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.TestSupport;
using Togawasakiko_in_Slay_the_Spire;

internal static class PressureCardRegression
{
    private static readonly (string Name, string PortraitPath)[] Cards =
    [
        ("UnspokenWords", "res://mod_assets/cards/normal/uncommon/unspoken_words.png"),
        ("LingeringResonance", "res://mod_assets/cards/normal/uncommon/lingering_resonance.png"),
        ("UntilNextAct", "res://mod_assets/cards/normal/uncommon/until_next_act.png"),
        ("ComposedResponse", "res://mod_assets/cards/normal/common/composed_response.png")
    ];
    private static readonly string[] Tokens = ["OverworkAnxiety", "SocialWithdrawal", "PersonaDissociation", "AllYouThinkAboutIsYourself"];

    private static void Equal<T>(T expected, T actual)
    {
        if (!Equals(expected, actual)) throw new Exception($"expected {expected}, got {actual}");
    }

    internal static async Task Run(Func<string, Func<Task>, Task> check)
    {
        TestLocalization.Load("eng");
        await check("Pressure bridge cards are registered once as non-Song rewards with real portraits", () =>
        {
            var assembly = typeof(TogawasakikoMod).Assembly;
            var pool = CombatFixture.Canonical<CardPoolModel>("TogawasakikoCardPool").AllCards;
            Type songType = assembly.GetType("Togawasakiko_in_Slay_the_Spire.ISongCard", true)!;
            Type support = assembly.GetType("Togawasakiko_in_Slay_the_Spire.ModSupport", true)!;
            var songs = (IReadOnlyList<CardModel>)support.GetMethod("GetSongPoolCanonicals")!.Invoke(null, null)!;
            Type character = assembly.GetType("Togawasakiko_in_Slay_the_Spire.Togawasakiko", true)!;
            foreach ((string name, string portraitPath) in Cards)
            {
                CardModel card = CombatFixture.Canonical<CardModel>(name);
                Equal(1, pool.Count(c => c.Id == card.Id));
                Equal(false, songType.IsInstanceOfType(card));
                Equal(false, songs.Any(c => c.Id == card.Id));
                Equal(true, character.GetMethod("IsRewardEligibleCard", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [card]));
                Equal(portraitPath, card.PortraitPath);
                Equal(true, File.Exists(Path.Combine(TestLocalization.RepoRoot, "mods/Togawasakiko_in_Slay_the_Spire/pack", card.PortraitPath[6..])));
            }
            Equal(26, songs.Count);
            Equal(63, pool.Count());
            Equal(17, pool.Count(card => card.Rarity == CardRarity.Common));
            Equal(33, pool.Count(card => card.Rarity == CardRarity.Uncommon));
            return Task.CompletedTask;
        });

        foreach (bool upgraded in new[] { false, true })
        {
            await check($"Unspoken Words selects own tokens only and grants native Block per exhaust: {upgraded}", async () =>
            {
                var f = new CombatFixture("Togawasakiko", "Togawasakiko");
                CardModel card = f.Card("UnspokenWords");
                if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
                CardModel[] tokens = Tokens.Select(name => f.Card(name)).ToArray();
                CardModel normal = f.Card("Slander"), song = f.Card("Ether"), ally = f.Card(Tokens[0], 1);
                foreach (var item in tokens.Concat([normal, song, ally])) await CardPileCmd.Add(item, PileType.Hand);
                using var select = CardSelectCmd.UseSelector(new Selection(tokens, [tokens[0], tokens[2]], 0, 4));
                ModelDb.Power<DexterityPower>().ToMutable().ApplyInternal(f.Players[0].Creature, 2, true);
                await f.PlayEffect(card);
                Equal(upgraded ? 18 : 14, f.Players[0].Creature.Block);
                Equal(2, f.Players[0].PlayerCombatState!.ExhaustPile.Cards.Count);
                Equal(4, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
                Equal(PileType.Hand, ally.Pile!.Type);
                Equal(false, f.Players[0].Creature.Powers.Any(p => p.Id.Entry == "PERSONA_DISSOCIATION_POWER"));
                Equal(false, card.Keywords.Contains(CardKeyword.Exhaust));
            });

            await check($"Until Next Act changes only selected combat tokens, preserving cost and Exhaust: {upgraded}", async () =>
            {
                var f = new CombatFixture("Togawasakiko", "Togawasakiko");
                CardModel card = f.Card("UntilNextAct");
                if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
                CardModel[] tokens = Tokens.Select(name => f.Card(name)).ToArray();
                CardModel song = f.Card("Ether"), ally = f.Card(Tokens[0], 1);
                foreach (var item in tokens.Concat([song, ally])) await CardPileCmd.Add(item, PileType.Hand);
                CardModel[] deckBefore = f.Players[0].Deck.Cards.ToArray();
                int count = upgraded ? 2 : 1;
                int[] costs = tokens.Select(CombatFixture.Cost).ToArray();
                using var select = CardSelectCmd.UseSelector(new Selection(tokens, tokens.Take(count).ToArray(), count, count));
                await f.PlayEffect(card);
                for (int i = 0; i < tokens.Length; i++)
                {
                    Equal(i >= count, tokens[i].Keywords.Contains(CardKeyword.Ethereal));
                    Equal(i < count, tokens[i].Keywords.Contains(CardKeyword.Retain));
                    Equal(true, tokens[i].Keywords.Contains(CardKeyword.Exhaust));
                    Equal(costs[i], CombatFixture.Cost(tokens[i]));
                }
                Equal(true, ally.Keywords.Contains(CardKeyword.Ethereal));
                Equal(false, ally.Keywords.Contains(CardKeyword.Retain));
                Equal(false, song.Keywords.Contains(CardKeyword.Retain));
                Equal(true, deckBefore.SequenceEqual(f.Players[0].Deck.Cards));
                foreach (string name in Tokens)
                {
                    Equal(true, CombatFixture.Canonical<CardModel>(name).Keywords.Contains(CardKeyword.Ethereal));
                    Equal(false, CombatFixture.Canonical<CardModel>(name).Keywords.Contains(CardKeyword.Retain));
                }
                Equal(true, card.Keywords.Contains(CardKeyword.Exhaust));
                f.StartCombat();
                Equal(true, f.Card(Tokens[0]).Keywords.Contains(CardKeyword.Ethereal));
                Equal(false, f.Card(Tokens[0]).Keywords.Contains(CardKeyword.Retain));
            });

            await check($"Lingering Resonance counts all own exhaustion including Ethereal, capped at two: {upgraded}", async () =>
            {
                var f = new CombatFixture("Togawasakiko", "Togawasakiko");
                await Activate(f, upgraded);
                await Exhaust(f, "Slander", player: 1);
                Equal(0, f.PressureAmount);
                await Exhaust(f, "Slander");
                Equal(upgraded ? 2 : 1, f.PressureAmount);
                await Exhaust(f, Tokens[0], ethereal: true);
                Equal(upgraded ? 4 : 2, f.PressureAmount);
                await Exhaust(f, "Ether");
                Equal(upgraded ? 4 : 2, f.PressureAmount);
            });

            foreach (int pressure in new[] { 0, 1, 4, 5, 8 })
            {
                await check($"Composed Response removes up to five, grants fixed native Block: pressure={pressure}, upgraded={upgraded}", async () =>
                {
                    var f = new CombatFixture();
                    if (pressure > 0) f.Pressure(pressure);
                    CardModel card = f.Card("ComposedResponse");
                    if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
                    ModelDb.Power<DexterityPower>().ToMutable().ApplyInternal(f.Players[0].Creature, 2, true);
                    await f.PlayEffect(card);
                    Equal(Math.Max(0, pressure - 5), f.PressureAmount);
                    Equal(upgraded ? 14 : 10, f.Players[0].Creature.Block);
                    Equal(0, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
                });
            }
        }

        await check("Unspoken Words permits zero selection and empty pool; Until Next Act accepts too few", async () =>
        {
            var f = new CombatFixture();
            await f.PlayEffect(f.Card("UnspokenWords"));
            await f.PlayEffect(f.Card("UntilNextAct"));
            CardModel token = f.Card(Tokens[0]);
            await CardPileCmd.Add(token, PileType.Hand);
            using (CardSelectCmd.UseSelector(new Selection([token], [], 0, 1)))
                await f.PlayEffect(f.Card("UnspokenWords"));
            Equal(0, f.Players[0].Creature.Block);
            Equal(PileType.Hand, token.Pile!.Type);
            CardModel card = f.Card("UntilNextAct");
            CardCmd.Upgrade(card, CardPreviewStyle.None);
            await f.PlayEffect(card);
            Equal(true, token.Keywords.Contains(CardKeyword.Retain));
            Equal(false, token.Keywords.Contains(CardKeyword.Ethereal));
        });

        await check("New pressure skills show the same Dexterity-modified Block that they grant", async () =>
        {
            foreach (string name in new[] { "UnspokenWords", "ComposedResponse" })
            {
                var f = new CombatFixture();
                CardModel card = f.Card(name);
                CardCmd.Upgrade(card, CardPreviewStyle.None);
                await CardPileCmd.Add(card, PileType.Hand);
                CardModel token = f.Card(Tokens[0]);
                await CardPileCmd.Add(token, PileType.Hand);
                using var select = CardSelectCmd.UseSelector(new Selection([token], [token], 0, 1));
                ModelDb.Power<DexterityPower>().ToMutable().ApplyInternal(f.Players[0].Creature, 2, true);
                card.UpdateDynamicVarPreview(CardPreviewMode.Normal, null, card.DynamicVars);
                int expected = name == "UnspokenWords" ? 9 : 14;
                Equal((decimal)expected, card.DynamicVars.Block.PreviewValue);
                Equal(true, Description(card).Contains(expected.ToString()));
                await f.PlayEffect(card);
                Equal(expected, f.Players[0].Creature.Block);
            }
        });

        await check("Unspoken Words snapshots selection before native Dark Embrace draws another token", async () =>
        {
            var f = new CombatFixture();
            CardModel first = f.Card(Tokens[0]), drawn = f.Card(Tokens[1]);
            await CardPileCmd.Add(first, PileType.Hand);
            await CardPileCmd.Add(drawn, PileType.Draw, CardPilePosition.Top);
            ModelDb.Power<DarkEmbracePower>().ToMutable().ApplyInternal(f.Players[0].Creature, 1, true);
            await Activate(f, false);
            using var select = CardSelectCmd.UseSelector(new Selection([first], [first], 0, 1));
            await f.PlayEffect(f.Card("UnspokenWords"));
            Equal(PileType.Exhaust, first.Pile!.Type);
            Equal(PileType.Hand, drawn.Pile!.Type);
            Equal(1, f.PressureAmount);
            Equal(5, f.Players[0].Creature.Block);
        });

        await check("Until Next Act excludes Imprisoned XII even after its actual on-entry draw", async () =>
        {
            var f = new CombatFixture();
            CardModel song = f.Card("ImprisonedXii");
            await CardPileCmd.Add(song, PileType.Hand);
            await f.PlayEffect(f.Card("UntilNextAct"));
            Equal(2, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
            Equal(false, song.Keywords.Contains(CardKeyword.Retain));
        });

        await check("Until Next Act survives native Ethereal and hand-flush phases; others expire", async () =>
        {
            var f = new CombatFixture();
            CardModel retained = f.Card(Tokens[0]), expiring = f.Card(Tokens[1]), normal = f.Card("Slander");
            foreach (var card in new[] { retained, expiring, normal }) await CardPileCmd.Add(card, PileType.Hand);
            using (CardSelectCmd.UseSelector(new Selection([retained, expiring], [retained], 1, 1)))
                await f.PlayEffect(f.Card("UntilNextAct"));
            await Activate(f, false);
            await (Task)typeof(CombatManager).GetMethod("DoTurnEnd", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(CombatManager.Instance, [f.Players[0], f.Choice])!;
            Equal(PileType.Exhaust, expiring.Pile!.Type);
            Equal(1, f.PressureAmount);
            Equal(PileType.Hand, retained.Pile!.Type);
            var context = new HookPlayerChoiceContext(f.Players[0], f.Players[0].NetId, GameActionType.CombatPlayPhaseOnly);
            await (Task)typeof(CombatManager).GetMethod("FlushPlayerHand", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(CombatManager.Instance, [f.Players[0], context])!;
            Equal(PileType.Hand, retained.Pile!.Type);
            Equal(PileType.Discard, normal.Pile!.Type);
            Equal(1, f.PressureAmount);
            await CardCmd.AutoPlay(f.Choice, retained, null, AutoPlayType.Default, true);
            Equal(PileType.Exhaust, retained.Pile!.Type);
            Equal(2, f.PressureAmount);
        });

        await check("Lingering affects every enemy; Composed Response consumes only the targeted enemy", async () =>
        {
            var f = new CombatFixture();
            Creature second = f.State.CreateCreature(ModelDb.Monster<TestSubject>().ToMutable(), CombatSide.Enemy, null);
            f.State.AddCreature(second);
            await Activate(f, true);
            await Exhaust(f, "Slander");
            Equal(2, f.PressureAmount);
            Equal(2, second.Powers.Single(p => p.Id.Entry == "PRESSURE_POWER").Amount);
            await f.PlayEffect(f.Card("ComposedResponse"));
            Equal(0, f.PressureAmount);
            Equal(2, second.Powers.Single(p => p.Id.Entry == "PRESSURE_POWER").Amount);
        });

        await check("Lingering never applies Pressure to a living allied Osty", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Necrobinder");
            Creature osty = await PlayerCmd.AddPet<Osty>(f.Players[1]);
            Equal(CombatSide.Player, osty.Side);
            Equal(true, osty.IsMonster);
            Equal(true, osty.IsAlive);
            await Activate(f, true);
            await Exhaust(f, "Slander");
            Equal(2, f.PressureAmount);
            Equal(false, osty.Powers.Any(p => p.Id.Entry == "PRESSURE_POWER"));
        });

        await check("Until Next Act itself exhausts via native autoplay and Choir, spending the same quota", async () =>
        {
            var f = new CombatFixture();
            await Activate(f, false);
            CardModel card = f.Card("UntilNextAct"), token = f.Card(Tokens[0]);
            await CardPileCmd.Add(token, PileType.Hand);
            await CardCmd.AutoPlay(f.Choice, card, null, AutoPlayType.Default, true);
            Equal(PileType.Exhaust, card.Pile!.Type);
            Equal(true, token.Keywords.Contains(CardKeyword.Retain));
            Equal(1, f.PressureAmount);
            await CardCmd.AutoPlay(f.Choice, f.Card("ChoirSChoir"), null, AutoPlayType.Default, true);
            Equal(PileType.Exhaust, card.Pile!.Type);
            Equal(2, f.PressureAmount);
            await CardCmd.AutoPlay(f.Choice, f.Card("ChoirSChoir"), null, AutoPlayType.Default, true);
            Equal(2, f.PressureAmount);
            Equal(PileType.Exhaust, card.Pile!.Type);
            Equal(3, CombatManager.Instance.History.CardPlaysFinished.Count(entry => ReferenceEquals(entry.CardPlay.Card, card)));
        });

        await check("Lingering Resonance stacks without renewing quota, resets only on owner turn and new combat", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Togawasakiko");
            await Exhaust(f, Tokens[0]);
            PowerModel power = await Activate(f, false);
            Equal(0, f.PressureAmount);
            await Exhaust(f, Tokens[0]);
            await Activate(f, true);
            await Exhaust(f, Tokens[1]);
            await Exhaust(f, Tokens[2]);
            Equal(4, f.PressureAmount);
            await power.BeforeSideTurnStart(f.Choice, CombatSide.Enemy, [f.Enemy], f.State);
            await power.BeforeSideTurnStart(f.Choice, CombatSide.Player, [f.Players[1].Creature], f.State);
            await Exhaust(f, Tokens[0]);
            Equal(4, f.PressureAmount);
            await power.BeforeSideTurnStart(f.Choice, CombatSide.Player, [f.Players[0].Creature], f.State);
            await Exhaust(f, Tokens[0]);
            Equal(7, f.PressureAmount);
            f.StartCombat();
            Equal(false, f.Players[0].Creature.Powers.Any(p => p.Id.Entry == "LINGERING_RESONANCE_POWER"));
            await Activate(f, false);
            await Exhaust(f, Tokens[0]);
            Equal(1, f.PressureAmount);
        });

        await check("Two Lingering owners have separate budgets on the shared Pressure counter", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Togawasakiko");
            await Activate(f, false);
            await Activate(f, true, 1);
            await Exhaust(f, Tokens[0]);
            await Exhaust(f, Tokens[0]);
            await Exhaust(f, Tokens[0]);
            Equal(2, f.PressureAmount);
            await Exhaust(f, Tokens[0], player: 1);
            await Exhaust(f, Tokens[0], player: 1);
            await Exhaust(f, Tokens[0], player: 1);
            Equal(6, f.PressureAmount);
        });

        await check("Choir replays real exhaust cards but cannot bypass Lingering's two-exhaust cap", async () =>
        {
            var f = new CombatFixture();
            await Activate(f, false);
            CardModel token = f.Card(Tokens[0]);
            await CardCmd.AutoPlay(f.Choice, token, null, AutoPlayType.Default, true);
            Equal(PileType.Exhaust, token.Pile!.Type);
            Equal(1, f.PressureAmount);
            await CardCmd.AutoPlay(f.Choice, f.Card("ChoirSChoir"), null, AutoPlayType.Default, true);
            Equal(2, f.PressureAmount);
            await CardCmd.AutoPlay(f.Choice, f.Card("ChoirSChoir"), null, AutoPlayType.Default, true);
            Equal(2, f.PressureAmount);
        });

        foreach (string language in new[] { "eng", "zhs" })
        {
            await check($"Retained tokens display exactly current native keywords, not stale Ethereal: {language}", async () =>
            {
                TestLocalization.Load(language);
                var f = new CombatFixture();
                foreach (string name in Tokens)
                {
                    CardModel token = f.Card(name);
                    await CardPileCmd.Add(token, PileType.Hand);
                    string ethereal = language == "eng" ? "Ethereal" : "虚无";
                    string retain = language == "eng" ? "Retain" : "保留";
                    string exhaust = language == "eng" ? "Exhaust" : "消耗";
                    Equal(1, Regex.Matches(Description(token), ethereal).Count);
                    Equal(1, Regex.Matches(Description(token), exhaust).Count);
                    await f.PlayEffect(f.Card("UntilNextAct"));
                    Equal(0, Regex.Matches(Description(token), ethereal).Count);
                    Equal(1, Regex.Matches(Description(token), retain).Count);
                    Equal(1, Regex.Matches(Description(token), exhaust).Count);
                    await CardPileCmd.Add(token, PileType.Exhaust);
                }
            });
            await check($"Pressure cards native upgrade/downgrade text, costs and keywords: {language}", () =>
            {
                TestLocalization.Load(language);
                var f = new CombatFixture();
                foreach ((string name, string variable, int before, int after) in new[] {
                    ("UnspokenWords", "Block", 5, 7), ("LingeringResonance", "Power", 1, 2),
                    ("UntilNextAct", "Cards", 1, 2), ("ComposedResponse", "Block", 8, 12) })
                {
                    CardModel card = f.Card(name);
                    string plain = Description(card);
                    Equal(true, plain.Contains(before.ToString()));
                    Equal((decimal)before, card.DynamicVars[variable].BaseValue);
                    CardCmd.Upgrade(card, CardPreviewStyle.None);
                    string upgraded = Description(card);
                    Equal(true, upgraded.Contains(after.ToString()));
                    Equal(false, plain == upgraded);
                    Equal((decimal)after, card.DynamicVars[variable].BaseValue);
                    CardCmd.Downgrade(card);
                    Equal(plain, Description(card));
                    Equal((decimal)before, card.DynamicVars[variable].BaseValue);
                    CardCmd.Upgrade(card, CardPreviewStyle.None);
                    Equal(upgraded, Description(card));
                    Equal((decimal)before, CombatFixture.Canonical<CardModel>(name).DynamicVars[variable].BaseValue);
                    Equal(name == "UntilNextAct" ? 0 : 1, CombatFixture.Cost(card));
                    Equal(name == "UntilNextAct", card.Keywords.Contains(CardKeyword.Exhaust));
                }
                return Task.CompletedTask;
            });
        }
        TestLocalization.Load("eng");
    }

    private static string Description(CardModel card) => Regex.Replace(card.GetDescriptionForPile(PileType.Hand), @"\[[^\]]+\]", "");

    private static async Task<PowerModel> Activate(CombatFixture f, bool upgraded, int player = 0)
    {
        CardModel card = f.Card("LingeringResonance", player);
        if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
        await f.PlayEffect(card);
        return f.Players[player].Creature.Powers.Single(p => p.Id.Entry == "LINGERING_RESONANCE_POWER");
    }

    private static async Task Exhaust(CombatFixture f, string name, bool ethereal = false, int player = 0)
    {
        CardModel card = f.Card(name, player);
        await CardPileCmd.Add(card, PileType.Hand);
        await CardCmd.Exhaust(f.Choice, card, ethereal);
    }

    private sealed class Selection(CardModel[] expected, CardModel[] chosen, int minimum, int maximum) : ICardSelector
    {
        public Task<IEnumerable<CardModel>> GetSelectedCards(IEnumerable<CardModel> options, int minSelect, int maxSelect)
        {
            Equal(minimum, minSelect);
            Equal(maximum, maxSelect);
            Equal(true, options.ToHashSet().SetEquals(expected));
            return Task.FromResult<IEnumerable<CardModel>>(chosen);
        }

        public CardRewardSelection GetSelectedCardReward(IReadOnlyList<CardCreationResult> options, IReadOnlyList<CardRewardAlternative> alternatives)
            => throw new NotSupportedException("No card reward choice in pressure card tests.");
    }
}
