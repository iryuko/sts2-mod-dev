using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using Togawasakiko_in_Slay_the_Spire;

internal static class PianoEventRegression
{
    private static int _portraits, _starts, _stops;
    private static bool Portrait() { _portraits++; return false; }
    private static bool Start() { _starts++; return false; }
    private static bool Stop() { _stops++; return false; }

    internal static async Task Run(Func<string, Func<Task>, Task> check)
    {
        TestLocalization.Load("eng");
        Type support = typeof(TogawasakikoMod).Assembly.GetType("Togawasakiko_in_Slay_the_Spire.ModSupport", true)!;
        var harmony = new Harmony("sakiko.piano-presentation-regression");
        foreach ((string method, string prefix) in new[] {
            ("TrySetCurrentEventPortrait", nameof(Portrait)),
            ("TryPlayShadowQuestionRoomEventMusic", nameof(Start)),
            ("StopShadowQuestionRoomEventMusic", nameof(Stop)) })
            harmony.Patch(AccessTools.Method(support, method), prefix: new HarmonyMethod(typeof(PianoEventRegression), prefix));
        ulong? previousIdentity = LocalContext.NetId;
        try
        {
            await check("Remote piano choices advance their event without changing local presentation", async () =>
            {
                var f = new CombatFixture("Togawasakiko", "Togawasakiko");
                LocalContext.NetId = f.Players[0].NetId;
                Reset();
                EventModel remote = await Begin(f.Players[1]);
                await Choose(remote, "PLAY");
                Equal(true, remote.CurrentOptions.Any(o => o.TextKey == "PLAY_1_CONTINUE"));
                Equal(0, _portraits);
                Equal(0, _starts);
                await Choose(remote, "PLAY_1_STOP");
                Equal(true, remote.IsFinished);
                Equal(0, _stops);
            });

            await check("Remote piano completion cannot stop local music; local cleanup runs once", async () =>
            {
                var f = new CombatFixture("Togawasakiko", "Togawasakiko");
                LocalContext.NetId = f.Players[0].NetId;
                Reset();
                EventModel local = await Begin(f.Players[0]), remote = await Begin(f.Players[1]);
                await Choose(local, "PLAY");
                Equal(1, _portraits);
                Equal(1, _starts);
                await Choose(remote, "LEAVE");
                Equal(true, remote.IsFinished);
                Equal(0, _stops);
                await Choose(local, "PLAY_1_STOP");
                local.EnsureCleanup();
                Equal(true, local.IsFinished);
                Equal(1, _stops);
            });

            await check("Piano without a local network identity still advances without presentation", async () =>
            {
                var f = new CombatFixture();
                LocalContext.NetId = null;
                Reset();
                EventModel model = await Begin(f.Players[0]);
                await Choose(model, "PLAY");
                Equal(2, model.CurrentOptions.Count);
                await Choose(model, "PLAY_1_STOP");
                Equal(true, model.IsFinished);
                Equal(0, _portraits + _starts + _stops);
            });

            await check("Piano grants all three Shadows to its owner and a fresh event restarts the full route", async () =>
            {
                var f = new CombatFixture("Togawasakiko", "Togawasakiko");
                LocalContext.NetId = f.Players[0].NetId;
                EventModel remote = await Begin(f.Players[1]);
                int hp = f.Players[1].Creature.CurrentHp;
                await Choose(remote, "PLAY");
                for (int stage = 1; stage <= 3; stage++) await Choose(remote, $"PLAY_{stage}_CONTINUE");
                Equal(hp - 18, f.Players[1].Creature.CurrentHp);
                Equal(3, f.Players[1].Deck.Cards.Count(c => c.Id.Entry.StartsWith("SHADOW_OF_THE_PAST_")));
                Equal(0, f.Players[0].Deck.Cards.Count(c => c.Id.Entry.StartsWith("SHADOW_OF_THE_PAST_")));
                Equal("FINAL_LEAVE", remote.CurrentOptions.Single().TextKey);
                await Choose(remote, "FINAL_LEAVE");
                EventModel reloaded = await Begin(f.Players[1]);
                await Choose(reloaded, "PLAY");
                Equal(true, reloaded.CurrentOptions.Any(o => o.TextKey == "PLAY_1_CONTINUE"));
            });
        }
        finally
        {
            LocalContext.NetId = previousIdentity;
            harmony.UnpatchAll(harmony.Id);
        }
    }

    private static async Task<EventModel> Begin(Player player)
    {
        EventModel model = CombatFixture.Canonical<EventModel>("UnattendedPiano").ToMutable();
        await model.BeginEvent(player, false);
        return model;
    }

    private static Task Choose(EventModel model, string key) => model.CurrentOptions.Single(option => option.TextKey == key).Chosen();
    private static void Reset() => _portraits = _starts = _stops = 0;
    private static void Equal<T>(T expected, T actual)
    {
        if (!Equals(expected, actual)) throw new Exception($"expected {expected}, got {actual}");
    }
}
