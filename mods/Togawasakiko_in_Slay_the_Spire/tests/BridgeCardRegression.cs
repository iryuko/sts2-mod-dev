using System.Reflection;
using System.Text.RegularExpressions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.TestSupport;
using Togawasakiko_in_Slay_the_Spire;

internal static class BridgeCardRegression
{
    private static readonly (string Name, string PortraitPath)[] Cards =
    [
        ("UnfinishedScore", "res://mod_assets/cards/normal/common/unfinished_score.png"),
        ("FollowingPhrase", "res://mod_assets/cards/normal/common/following_phrase.png"),
        ("Unmask", "res://mod_assets/cards/normal/uncommon/unmask.png"),
        ("RehearsalOrder", "res://mod_assets/cards/normal/uncommon/rehearsal_order.png"),
        ("BackstageSupport", "res://mod_assets/cards/normal/uncommon/backstage_support.png")
    ];
    private static void Equal<T>(T expected, T actual)
    {
        if (!Equals(expected, actual)) throw new Exception($"expected {expected}, got {actual}");
    }

    internal static async Task Run(Func<string, Func<Task>, Task> check)
    {
        TestLocalization.Load("eng");
        await check("Bridge cards belong to rewards, not Song generation; portraits exist", () =>
        {
            Type assemblyType = typeof(TogawasakikoMod);
            Type songType = assemblyType.Assembly.GetType("Togawasakiko_in_Slay_the_Spire.ISongCard", true)!;
            Type support = assemblyType.Assembly.GetType("Togawasakiko_in_Slay_the_Spire.ModSupport", true)!;
            var generatedSongs = (IReadOnlyList<CardModel>)support.GetMethod("GetSongPoolCanonicals")!.Invoke(null, null)!;
            Type character = assemblyType.Assembly.GetType("Togawasakiko_in_Slay_the_Spire.Togawasakiko", true)!;
            var pool = CombatFixture.Canonical<CardPoolModel>("TogawasakikoCardPool").AllCards;
            foreach ((string name, string portraitPath) in Cards)
            {
                CardModel card = CombatFixture.Canonical<CardModel>(name);
                Equal(1, pool.Count(c => c.Id == card.Id));
                Equal(false, songType.IsInstanceOfType(card));
                Equal(false, generatedSongs.Any(song => song.Id == card.Id));
                Equal(true, character.GetMethod("IsRewardEligibleCard", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [card]));
                Equal(portraitPath, card.PortraitPath);
                Equal(true, File.Exists(Path.Combine(TestLocalization.RepoRoot, "mods/Togawasakiko_in_Slay_the_Spire/pack", card.PortraitPath[6..])));
            }
            return Task.CompletedTask;
        });

        foreach (bool upgraded in new[] { false, true })
        {
            await check($"Unfinished Score selects own Song, keeps instance and gains native Block; upgraded={upgraded}", async () =>
            {
                var f = new CombatFixture("Togawasakiko", "Togawasakiko");
                CardModel[] deckBefore = f.Players[0].Deck.Cards.ToArray();
                CardModel card = f.Card("UnfinishedScore");
                if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
                CardModel song = f.Card("SymbolIii");
                CardCmd.Upgrade(song, CardPreviewStyle.None);
                CardModel otherSong = f.Card("Ether");
                CardModel nonSong = f.Card("Slander");
                CardModel allySong = f.Card("Face", 1);
                foreach (CardModel candidate in new[] { song, otherSong, nonSong, allySong }) await CardPileCmd.Add(candidate, PileType.Discard);
                using var select = CardSelectCmd.UseSelector(new ExactSelector(song, [song, otherSong]));
                ModelDb.Power<DexterityPower>().ToMutable().ApplyInternal(f.Players[0].Creature, 2, true);
                await f.PlayEffect(card);
                Equal(upgraded ? 10 : 7, f.Players[0].Creature.Block);
                Equal(song, f.Players[0].PlayerCombatState!.DrawPile.Cards.First());
                Equal(true, song.IsUpgraded);
                Equal(PileType.Discard, nonSong.Pile!.Type);
                Equal(PileType.Discard, allySong.Pile!.Type);
                Equal(true, deckBefore.SequenceEqual(f.Players[0].Deck.Cards));
            });
        }

        await check("Unfinished Score empty/no-Song discard needs no choice and still blocks", async () =>
        {
            var f = new CombatFixture();
            await EmptyDraw(f);
            await f.PlayEffect(f.Card("UnfinishedScore"));
            await CardPileCmd.Add(f.Card("Slander"), PileType.Discard);
            await f.PlayEffect(f.Card("UnfinishedScore"));
            Equal(10, f.Players[0].Creature.Block);
            Equal(0, f.Players[0].PlayerCombatState!.DrawPile.Cards.Count);
        });

        foreach (bool upgraded in new[] { false, true })
        {
            await check($"Following Phrase uses own immediate Song and native Block; upgraded={upgraded}", async () =>
            {
                var f = new CombatFixture("Togawasakiko", "Togawasakiko");
                await FillDraw(f, 3);
                await f.Finish(f.Card("Ether"));
                await f.Finish(f.Card("Slander", 1));
                CardModel card = f.Card("FollowingPhrase");
                if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
                ModelDb.Power<DexterityPower>().ToMutable().ApplyInternal(f.Players[0].Creature, 1, true);
                await f.PlayEffect(card);
                Equal(upgraded ? 10 : 7, f.Players[0].Creature.Block);
                Equal(1, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
                await f.Finish(card);
                await f.PlayEffect(f.Card("FollowingPhrase"));
                Equal(1, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
            });
        }

        await check("Following Phrase excludes another player's Song and clears on owner turn", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Togawasakiko");
            await FillDraw(f, 3);
            await f.Finish(f.Card("Ether", 1));
            await f.PlayEffect(f.Card("FollowingPhrase"));
            Equal(0, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
            await f.Finish(f.Card("Ether"));
            f.State.RoundNumber++;
            foreach (PowerModel power in f.Players[0].Creature.Powers.ToList())
                await power.AfterPlayerTurnStartEarly(f.Choice, f.Players[0]);
            await f.PlayEffect(f.Card("FollowingPhrase"));
            Equal(0, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
        });

        await check("Following Phrase counts Choir before nested autoplay and not after it finishes", async () =>
        {
            var f = new CombatFixture();
            await f.Finish(f.Card("Slander"));
            await CardPileCmd.Add(f.Card("FollowingPhrase"), PileType.Exhaust);
            await CardPileCmd.Add(f.Card("FollowingPhrase"), PileType.Exhaust);
            await CardCmd.AutoPlay(f.Choice, f.Card("ChoirSChoir"), null, AutoPlayType.Default, true);
            Equal(1, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
            Equal(12, f.Players[0].Creature.Block);
            await f.PlayEffect(f.Card("FollowingPhrase"));
            Equal(1, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
        });

        await check("Following Phrase repeated by native Burst only follows Song on first play", async () =>
        {
            var f = new CombatFixture();
            await f.Finish(f.Card("Ether"));
            ModelDb.Power<BurstPower>().ToMutable().ApplyInternal(f.Players[0].Creature, 1, true);
            await CardCmd.AutoPlay(f.Choice, f.Card("FollowingPhrase"), null, AutoPlayType.Default, true);
            Equal(1, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
            Equal(12, f.Players[0].Creature.Block);
        });

        foreach (bool upgraded in new[] { false, true })
        {
            await check($"Unmask removes ALL own Face but grants bonus only once; upgraded={upgraded}", async () =>
            {
                var f = new CombatFixture("Togawasakiko", "Togawasakiko");
                await FillDraw(f, 3);
                CombatFixture.Canonical<PowerModel>("FaceReactionPower").ToMutable().ApplyInternal(f.Players[0].Creature, 3, true);
                CombatFixture.Canonical<PowerModel>("FaceReactionPower").ToMutable().ApplyInternal(f.Players[1].Creature, 2, true);
                CardModel card = f.Card("Unmask");
                if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
                await f.PlayEffect(card);
                Equal(2, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
                Equal(upgraded ? 5 : 3, f.PressureAmount);
                Equal(false, f.Players[0].Creature.Powers.Any(p => p.Id.Entry == "FACE_REACTION_POWER"));
                Equal(2, f.Players[1].Creature.Powers.Single(p => p.Id.Entry == "FACE_REACTION_POWER").Amount);
                Equal(true, card.Keywords.Contains(CardKeyword.Exhaust));
            });
        }

        await check("Unmask without Face only draws one; empty draw still removes Face", async () =>
        {
            var f = new CombatFixture();
            await FillDraw(f, 1);
            await f.PlayEffect(f.Card("Unmask"));
            Equal(1, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
            Equal(0, f.PressureAmount);
            CombatFixture.Canonical<PowerModel>("FaceReactionPower").ToMutable().ApplyInternal(f.Players[0].Creature, 2, true);
            await f.PlayEffect(f.Card("Unmask"));
            Equal(3, f.PressureAmount);
            Equal(false, f.Players[0].Creature.Powers.Any(p => p.Id.Entry == "FACE_REACTION_POWER"));
        });

        await check("Rehearsal Order filters rarity/Song/owner and moves, never copies", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Togawasakiko");
            CardModel[] deckBefore = f.Players[0].Deck.Cards.ToArray();
            CardModel chosen = f.Card("SymbolIii");
            CardCmd.Upgrade(chosen, CardPreviewStyle.None);
            CardModel common = f.Card("Face");
            CardModel rare = f.Card("KillKiss");
            CardModel nonSong = f.Card("Compose");
            CardModel ally = f.Card("Ether", 1);
            foreach (CardModel card in new[] { chosen, common, rare, nonSong, ally }) await CardPileCmd.Add(card, PileType.Draw);
            using var select = CardSelectCmd.UseSelector(new ExactSelector(chosen, [chosen, common]));
            await f.PlayEffect(f.Card("RehearsalOrder"));
            Equal(chosen, f.Players[0].PlayerCombatState!.Hand.Cards.Single());
            Equal(true, chosen.IsUpgraded);
            Equal(PileType.Draw, rare.Pile!.Type);
            Equal(PileType.Draw, nonSong.Pile!.Type);
            Equal(PileType.Draw, ally.Pile!.Type);
            Equal(true, deckBefore.SequenceEqual(f.Players[0].Deck.Cards));
        });

        await check("Rehearsal Order with no eligible Song completes without choice", async () =>
        {
            var f = new CombatFixture();
            await EmptyDraw(f);
            await f.PlayEffect(f.Card("RehearsalOrder"));
            await CardPileCmd.Add(f.Card("KillKiss"), PileType.Draw);
            await f.PlayEffect(f.Card("RehearsalOrder"));
            Equal(0, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
        });

        await check("Rehearsal Order preserves Imprisoned XII's real on-entry draw", async () =>
        {
            var f = new CombatFixture();
            await FillDraw(f, 2);
            CardModel song = f.Card("ImprisonedXii");
            CardCmd.Upgrade(song, CardPreviewStyle.None);
            await CardPileCmd.Add(song, PileType.Draw);
            await f.PlayEffect(f.Card("RehearsalOrder"));
            Equal(3, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
            Equal(1, f.Players[0].PlayerCombatState!.Hand.Cards.Count(c => ReferenceEquals(song, c)));
            Equal(0, f.Players[0].PlayerCombatState!.DrawPile.Cards.Count);
        });

        await check("Bridge skill Block preview matches Dexterity-modified actual Block", async () =>
        {
            foreach ((string name, int expected) in new[] { ("UnfinishedScore", 10), ("FollowingPhrase", 11) })
            {
                var f = new CombatFixture();
                CardModel card = f.Card(name);
                CardCmd.Upgrade(card, CardPreviewStyle.None);
                await CardPileCmd.Add(card, PileType.Hand);
                ModelDb.Power<DexterityPower>().ToMutable().ApplyInternal(f.Players[0].Creature, 2, true);
                card.UpdateDynamicVarPreview(CardPreviewMode.Normal, null, card.DynamicVars);
                Equal((decimal)expected, card.DynamicVars.Block.PreviewValue);
                Equal(true, PlainDescription(card).Contains(expected.ToString()));
                await f.PlayEffect(card);
                Equal(expected, f.Players[0].Creature.Block);
            }
        });

        foreach (bool upgraded in new[] { false, true })
        {
            await check($"Backstage Support caps own tokens at two including autoplay; upgraded={upgraded}", async () =>
            {
                var f = new CombatFixture("Togawasakiko", "Togawasakiko");
                PowerModel power = await ActivateSupport(f, upgraded);
                ModelDb.Power<DexterityPower>().ToMutable().ApplyInternal(f.Players[0].Creature, 5, true);
                await f.Finish(f.Card("OverworkAnxiety", 1));
                await f.Finish(f.Card("Slander"));
                await f.Finish(f.Card("Ether"));
                Equal(0, f.Players[0].Creature.Block);
                await f.Finish(f.Card("OverworkAnxiety"));
                Equal(upgraded ? 4 : 3, f.Players[0].Creature.Block);
                await f.Finish(f.Card("SocialWithdrawal"), true);
                Equal(upgraded ? 8 : 6, f.Players[0].Creature.Block);
                await f.Finish(f.Card("PersonaDissociation"));
                Equal(upgraded ? 8 : 6, f.Players[0].Creature.Block);
                await power.BeforeSideTurnStart(f.Choice, CombatSide.Enemy, [f.Enemy], f.State);
                await f.Finish(f.Card("OverworkAnxiety"));
                Equal(upgraded ? 8 : 6, f.Players[0].Creature.Block);
                await power.BeforeSideTurnStart(f.Choice, CombatSide.Player, [f.Players[0].Creature], f.State);
                await f.Finish(f.Card("AllYouThinkAboutIsYourself"));
                Equal(upgraded ? 12 : 9, f.Players[0].Creature.Block);
            });
        }

        await check("Backstage Support ignores Ethereal exhaustion, stacks without refreshing budget", async () =>
        {
            var f = new CombatFixture();
            await ActivateSupport(f, false);
            CardModel expiring = f.Card("OverworkAnxiety");
            await CardPileCmd.Add(expiring, PileType.Hand);
            await CardCmd.Exhaust(f.Choice, expiring, true);
            Equal(0, f.Players[0].Creature.Block);
            await f.Finish(f.Card("OverworkAnxiety"));
            Equal(3, f.Players[0].Creature.Block);
            await ActivateSupport(f, true);
            await f.Finish(f.Card("OverworkAnxiety"));
            Equal(10, f.Players[0].Creature.Block);
            await f.Finish(f.Card("OverworkAnxiety"));
            Equal(10, f.Players[0].Creature.Block);
            f.StartCombat();
            Equal(false, f.Players[0].Creature.Powers.Any(p => p.Id.Entry == "BACKSTAGE_SUPPORT_POWER"));
            await ActivateSupport(f, false);
            await f.Finish(f.Card("OverworkAnxiety"));
            Equal(3, f.Players[0].Creature.Block);
        });

        await check("Two Backstage owners keep independent budgets; activation never rewards prior plays", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Togawasakiko");
            await f.Finish(f.Card("OverworkAnxiety"));
            await ActivateSupport(f, false);
            CardModel allySupport = f.Card("BackstageSupport", 1);
            CardCmd.Upgrade(allySupport, CardPreviewStyle.None);
            await f.PlayEffect(allySupport);
            Equal(0, f.Players[0].Creature.Block);
            await f.Finish(f.Card("OverworkAnxiety"));
            await f.Finish(f.Card("OverworkAnxiety"));
            await f.Finish(f.Card("OverworkAnxiety", 1));
            Equal(6, f.Players[0].Creature.Block);
            Equal(4, f.Players[1].Creature.Block);
            await f.Finish(f.Card("OverworkAnxiety"));
            await f.Finish(f.Card("OverworkAnxiety", 1));
            Equal(6, f.Players[0].Creature.Block);
            Equal(8, f.Players[1].Creature.Block);
        });

        await check("Native Burst replays one pressure token twice and exhausts Backstage's budget", async () =>
        {
            var f = new CombatFixture();
            await ActivateSupport(f, false);
            ModelDb.Power<BurstPower>().ToMutable().ApplyInternal(f.Players[0].Creature, 1, true);
            CardModel repeated = f.Card("OverworkAnxiety");
            await CardCmd.AutoPlay(f.Choice, repeated, null, AutoPlayType.Default, true);
            Equal(6, f.Players[0].Creature.Block);
            Equal(PileType.Exhaust, repeated.Pile!.Type);
            Equal(2, CombatManager.Instance.History.CardPlaysFinished.Count(e => ReferenceEquals(e.CardPlay.Card, repeated)));
            await CardCmd.AutoPlay(f.Choice, f.Card("OverworkAnxiety"), null, AutoPlayType.Default, true);
            Equal(6, f.Players[0].Creature.Block);
        });

        foreach (string language in new[] { "eng", "zhs" })
        {
            await check($"Bridge cards native upgrade/downgrade descriptions and keywords: {language}", () =>
            {
                TestLocalization.Load(language);
                var f = new CombatFixture();
                foreach ((string name, string variable, int before, int after) in new[] {
                    ("UnfinishedScore", "Block", 5, 8), ("FollowingPhrase", "Block", 6, 9),
                    ("Unmask", "PressureAmount", 3, 5), ("BackstageSupport", "Power", 3, 4) })
                {
                    CardModel card = f.Card(name);
                    string plain = PlainDescription(card);
                    Equal(true, plain.Contains(before.ToString()));
                    Equal((decimal)before, card.DynamicVars[variable].BaseValue);
                    CardCmd.Upgrade(card, CardPreviewStyle.None);
                    string upgraded = PlainDescription(card);
                    Equal(true, upgraded.Contains(after.ToString()));
                    Equal(false, plain == upgraded);
                    Equal((decimal)after, card.DynamicVars[variable].BaseValue);
                    CardCmd.Downgrade(card);
                    Equal(plain, PlainDescription(card));
                    Equal((decimal)before, card.DynamicVars[variable].BaseValue);
                    CardCmd.Upgrade(card, CardPreviewStyle.None);
                    Equal(upgraded, PlainDescription(card));
                    Equal((decimal)before, CombatFixture.Canonical<CardModel>(name).DynamicVars[variable].BaseValue);
                }
                foreach (string name in new[] { "Unmask", "RehearsalOrder" })
                {
                    CardModel card = f.Card(name);
                    string keyword = language == "eng" ? "Exhaust" : "消耗";
                    Equal(1, Regex.Matches(PlainDescription(card), keyword).Count);
                    CardCmd.Upgrade(card, CardPreviewStyle.None);
                    Equal(0, CombatFixture.Cost(card));
                    Equal(1, Regex.Matches(PlainDescription(card), keyword).Count);
                    CardCmd.Downgrade(card);
                    Equal(name == "RehearsalOrder" ? 1 : 0, CombatFixture.Cost(card));
                    Equal(1, Regex.Matches(PlainDescription(card), keyword).Count);
                }
                return Task.CompletedTask;
            });
        }
        TestLocalization.Load("eng");
    }

    private static string PlainDescription(CardModel card) => Regex.Replace(card.GetDescriptionForPile(PileType.Hand), @"\[[^\]]+\]", "");

    private static async Task FillDraw(CombatFixture f, int count)
    {
        await EmptyDraw(f);
        for (int i = 0; i < count; i++) await CardPileCmd.Add(f.Card("Slander"), PileType.Draw);
    }

    private static async Task EmptyDraw(CombatFixture f)
    {
        foreach (CardModel card in f.Players[0].PlayerCombatState!.DrawPile.Cards.ToList())
            await CardPileCmd.Add(card, PileType.Exhaust);
    }

    private static async Task<PowerModel> ActivateSupport(CombatFixture f, bool upgraded)
    {
        CardModel card = f.Card("BackstageSupport");
        if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
        await f.PlayEffect(card);
        return f.Players[0].Creature.Powers.Single(p => p.Id.Entry == "BACKSTAGE_SUPPORT_POWER");
    }

    private sealed class ExactSelector(CardModel chosen, CardModel[] expectedOptions) : ICardSelector
    {
        public Task<IEnumerable<CardModel>> GetSelectedCards(IEnumerable<CardModel> options, int minSelect, int maxSelect)
        {
            Equal(1, minSelect);
            Equal(1, maxSelect);
            Equal(true, options.ToHashSet().SetEquals(expectedOptions));
            return Task.FromResult<IEnumerable<CardModel>>([chosen]);
        }

        public CardRewardSelection GetSelectedCardReward(IReadOnlyList<CardCreationResult> options, IReadOnlyList<CardRewardAlternative> alternatives)
            => throw new NotSupportedException("No reward selection in bridge card tests.");
    }
}
