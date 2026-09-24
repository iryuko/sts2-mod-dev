using System.Reflection;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Togawasakiko_in_Slay_the_Spire;

internal static class ShadowMigrationRegression
{
    internal static async Task Run(Func<string, Func<Task>, Task> check)
    {
        // The managed host registers models, but does not run the mod initializer.
        typeof(TogawasakikoMod).GetMethod("RegisterSavedPropertyTypes", BindingFlags.Static | BindingFlags.NonPublic)!
            .Invoke(null, null);

        foreach (string name in new[] { "ShadowOfThePastI", "ShadowOfThePastII", "ShadowOfThePastIII" })
        {
            await check($"{name} migration preserves native saved metadata in every player's deck", () =>
            {
                var f = new CombatFixture("Togawasakiko", "Togawasakiko", "Silent");
                int[] floors = [0, 7, 23];
                int[] progress = [0, 1, 1];
                var previous = new CardModel[f.Players.Length];
                for (int i = 0; i < f.Players.Length; i++)
                {
                    CardModel seed = CombatFixture.Canonical<CardModel>(name).ToMutable();
                    seed.FloorAddedToDeck = floors[i];
                    SetCombatsSeen(seed, progress[i]);
                    previous[i] = f.Run.LoadCard(seed.ToSerializable(), f.Players[i]);
                    AssertMetadata(previous[i], floors[i], progress[i]);
                    f.Players[i].Deck.AddInternal(previous[i], 1, silent: true);
                }
                CardModel[][] decks = f.Players.Select(player => player.Deck.Cards.ToArray()).ToArray();

                for (int round = 0; round < 2; round++)
                {
                    Sanitize(f.Run);
                    for (int i = 0; i < f.Players.Length; i++)
                    {
                        var player = f.Players[i];
                        CardModel fresh = player.Deck.Cards[1];
                        Equal(false, ReferenceEquals(previous[i], fresh));
                        Equal(previous[i].Id, fresh.Id);
                        Equal(player, fresh.Owner);
                        Equal(false, f.Run.ContainsCard(previous[i]));
                        Equal(true, f.Run.ContainsCard(fresh));
                        Equal(decks[i].Length, player.Deck.Cards.Count);
                        for (int index = 0; index < decks[i].Length; index++)
                            if (index != 1) Equal(true, ReferenceEquals(decks[i][index], player.Deck.Cards[index]));
                        AssertMetadata(fresh, floors[i], progress[i]);

                        var saved = fresh.ToSerializable();
                        Equal<int?>(floors[i], saved.FloorAddedToDeck);
                        CardModel reloaded = CardModel.FromSerializable(saved);
                        AssertMetadata(reloaded, floors[i], progress[i]);
                        Equal(false, ReferenceEquals(fresh, reloaded));
                        Equal(false, ReferenceEquals(fresh.DynamicVars["Combats"], reloaded.DynamicVars["Combats"]));
                        previous[i] = fresh;
                    }
                }
                AssertMetadata(CombatFixture.Canonical<CardModel>(name), null, 0);
                return Task.CompletedTask;
            });

            await check($"{name} migration preserves null floors without sharing canonical or instance state", () =>
            {
                var f = new CombatFixture("Togawasakiko", "Silent");
                CardModel canonical = CombatFixture.Canonical<CardModel>(name);
                CardModel seed = canonical.ToMutable();
                SetCombatsSeen(seed, 1);
                var saved = seed.ToSerializable();
                CardModel[] previous =
                [
                    f.Run.LoadCard(saved, f.Players[0]),
                    f.Run.LoadCard(saved, f.Players[0]),
                    f.Run.LoadCard(saved, f.Players[1])
                ];
                f.Players[0].Deck.AddInternal(previous[0], 1, silent: true);
                f.Players[0].Deck.AddInternal(previous[1], 2, silent: true);
                f.Players[1].Deck.AddInternal(previous[2], 1, silent: true);
                Sanitize(f.Run);
                CardModel[] fresh = [f.Players[0].Deck.Cards[1], f.Players[0].Deck.Cards[2], f.Players[1].Deck.Cards[1]];
                for (int i = 0; i < fresh.Length; i++)
                {
                    AssertMetadata(fresh[i], null, 1);
                    Equal(false, ReferenceEquals(fresh[i], previous[i]));
                    Equal(false, ReferenceEquals(fresh[i], canonical));
                    Equal(false, ReferenceEquals(fresh[i].DynamicVars["Combats"], canonical.DynamicVars["Combats"]));
                    for (int j = 0; j < i; j++)
                    {
                        Equal(false, ReferenceEquals(fresh[i], fresh[j]));
                        Equal(false, ReferenceEquals(fresh[i].DynamicVars["Combats"], fresh[j].DynamicVars["Combats"]));
                    }
                }

                CardModel reloaded = CardModel.FromSerializable(fresh[0].ToSerializable());
                fresh[0].FloorAddedToDeck = 9;
                SetCombatsSeen(fresh[0], 2);
                AssertMetadata(fresh[0], 9, 2);
                AssertMetadata(fresh[1], null, 1);
                AssertMetadata(fresh[2], null, 1);
                AssertMetadata(reloaded, null, 1);
                foreach (CardModel old in previous) AssertMetadata(old, null, 1);
                AssertMetadata(seed, null, 1);
                AssertMetadata(canonical, null, 0);

                reloaded.FloorAddedToDeck = 5;
                SetCombatsSeen(reloaded, 0);
                AssertMetadata(fresh[0], 9, 2);
                AssertMetadata(CardModel.FromSerializable(fresh[0].ToSerializable()), 9, 2);
                AssertMetadata(canonical, null, 0);
                return Task.CompletedTask;
            });
        }
    }

    private static void Sanitize(RunState run)
    {
        FieldInfo flag = typeof(TogawasakikoMod).GetField("_shadowDeckSanitizedForActiveRun", BindingFlags.Static | BindingFlags.NonPublic)!;
        object? previous = flag.GetValue(null);
        try
        {
            typeof(TogawasakikoMod).GetMethod("SanitizeShadowCards", BindingFlags.Static | BindingFlags.NonPublic)!
                .Invoke(null, [run]);
        }
        finally
        {
            flag.SetValue(null, previous);
        }
    }

    private static void SetCombatsSeen(CardModel card, int value) => card.GetType().GetProperty("CombatsSeen")!.SetValue(card, value);

    private static void AssertMetadata(CardModel card, int? floor, int combats)
    {
        if (card.FloorAddedToDeck != floor)
            throw new Exception($"{card.Id.Entry}: FloorAddedToDeck expected {floor?.ToString() ?? "null"}, got {card.FloorAddedToDeck?.ToString() ?? "null"}.");
        Equal(combats, (int)card.GetType().GetProperty("CombatsSeen")!.GetValue(card)!);
        Equal((decimal)(2 - combats), card.DynamicVars["Combats"].BaseValue);
    }

    private static void Equal<T>(T expected, T actual)
    {
        if (!Equals(expected, actual)) throw new Exception($"expected {expected}, got {actual}");
    }
}
