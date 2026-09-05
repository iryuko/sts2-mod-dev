using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using Togawasakiko_in_Slay_the_Spire;

internal static class OctagramReplayRegression
{
    private static void Equal<T>(T expected, T actual)
    {
        if (!Equals(expected, actual)) throw new Exception($"expected {expected}, got {actual}");
    }

    internal static async Task Run(Func<string, Func<Task>, Task> check)
    {
        TestLocalization.Load("eng");
        await check("Octagram starts a fresh Skill sequence and the violating card resolves before owner readiness", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Togawasakiko");
            using var host = new ReadinessHost();
            await f.Finish(f.Card("DefendTogawasakiko"));
            await CardCmd.AutoPlay(f.Choice, f.Card("OctagramDance"), null);
            Equal(false, CombatManager.Instance.IsPlayerReadyToEndTurn(f.Players[0]));
            CardModel defend = f.Card("DefendTogawasakiko");
            await CardCmd.AutoPlay(f.Choice, defend, null);
            Equal(5, f.Players[0].Creature.Block);
            Equal(true, CombatManager.Instance.IsPlayerReadyToEndTurn(f.Players[0]));
            Equal(false, CombatManager.Instance.IsPlayerReadyToEndTurn(f.Players[1]));
            Equal(PileType.Discard, defend.Pile!.Type);
            Equal(false, CombatManager.Instance.IsExecutingCardOrPotionEffect(f.Players[0]));
        });

        await check("Octagram permits real type alternation and reapplication does not reset the sequence", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Togawasakiko");
            using var host = new ReadinessHost();
            await CardCmd.AutoPlay(f.Choice, f.Card("OctagramDance"), null);
            await CardCmd.AutoPlay(f.Choice, f.Card("Ether"), f.Enemy);
            await CardCmd.AutoPlay(f.Choice, f.Card("Divine"), null);
            await CardCmd.AutoPlay(f.Choice, f.Card("AveMujica"), null);
            await CardCmd.AutoPlay(f.Choice, f.Card("OctagramDance"), null);
            Equal(false, CombatManager.Instance.IsPlayerReadyToEndTurn(f.Players[0]));
            Equal(1, f.Players[0].Creature.Powers.Count(p => p.Id.Entry == "OCTAGRAM_DANCE_POWER"));
            await CardCmd.AutoPlay(f.Choice, f.Card("Divine"), null);
            Equal(true, CombatManager.Instance.IsPlayerReadyToEndTurn(f.Players[0]));
            Equal(false, CombatManager.Instance.IsPlayerReadyToEndTurn(f.Players[1]));
        });

        await check("Octagram violation inside Blue Eyes skips the unstarted second source without moving it", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Togawasakiko");
            using var host = new ReadinessHost();
            await CardCmd.AutoPlay(f.Choice, f.Card("OctagramDance"), null);
            await CardCmd.AutoPlay(f.Choice, f.Card("Ether"), f.Enemy);
            CardModel blue = f.Card("InYourBlueEyes"), exhaust = f.Card("Ether"), discard = f.Card("SymbolIii");
            CardCmd.Upgrade(blue, MegaCrit.Sts2.Core.Nodes.CommonUi.CardPreviewStyle.None);
            // Remove both setup cards so each Song source is unambiguous.
            foreach (CardModel card in f.Players[0].PlayerCombatState!.DiscardPile.Cards
                .Concat(f.Players[0].PlayerCombatState!.ExhaustPile.Cards).ToArray())
                await CardPileCmd.Add(card, PileType.Draw);
            await CardPileCmd.Add(exhaust, PileType.Exhaust);
            await CardPileCmd.Add(discard, PileType.Discard);
            await CardCmd.AutoPlay(f.Choice, blue, null);
            Equal(2, CombatManager.Instance.History.CardPlaysFinished.Count(e => e.CardPlay.Card == exhaust));
            Equal(0, CombatManager.Instance.History.CardPlaysFinished.Count(e => e.CardPlay.Card == discard));
            Equal(true, CombatManager.Instance.IsPlayerReadyToEndTurn(f.Players[0]));
            Equal(PileType.Discard, discard.Pile!.Type);
            Equal(PileType.Discard, blue.Pile!.Type);
            Equal(0, f.Players[0].PlayerCombatState!.PlayPile.Cards.Count);
        });

        await check("Octagram native replay transpiler installs and rejects incompatible IL", () =>
        {
            Type patch = typeof(TogawasakikoMod).Assembly.GetType("Togawasakiko_in_Slay_the_Spire.OctagramReplayPatches", true)!;
            MethodBase target = (MethodBase)patch.GetMethod("TargetMethod", BindingFlags.Static | BindingFlags.NonPublic)!
                .Invoke(null, null)!;
            Equal(true, Harmony.GetPatchInfo(target)?.Transpilers.Any(p => p.PatchMethod.DeclaringType == patch) == true);
            try
            {
                patch.GetMethod("Transpiler", BindingFlags.Static | BindingFlags.NonPublic)!
                    .Invoke(null, [Array.Empty<CodeInstruction>(), target]);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is InvalidOperationException failure
                && failure.Message.Contains("exactly one validated native replay-loop boundary"))
            {
                return Task.CompletedTask;
            }
            throw new Exception("Incompatible native IL was not rejected clearly.");
        });

        await check("Octagram real violation readies only its owner (local host-type service stub)", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Togawasakiko");
            using var host = new ReadinessHost();
            ApplyPower(f);
            CardModel card = f.State.CreateCard(ModelDb.Card<Whirlwind>(), f.Players[0]);
            card.BaseReplayCount = 2;
            card.EnergyCost.CapturedXValue = 0;
            await CardCmd.AutoPlay(f.Choice, card, null, skipXCapture: true, skipCardPileVisuals: true);
            Equal(2, CombatManager.Instance.History.CardPlaysFinished.Count(e => e.CardPlay.Card == card));
            Equal(true, CombatManager.Instance.IsPlayerReadyToEndTurn(f.Players[0]));
            Equal(false, CombatManager.Instance.IsPlayerReadyToEndTurn(f.Players[1]));
            Equal(false, CombatManager.Instance.AllPlayersReadyToEndTurn());
            Equal(CombatSide.Player, f.State.CurrentSide);
            Equal(PileType.Discard, card.Pile!.Type);
            Equal(true, f.State.CreateCard(ModelDb.Card<Whirlwind>(), f.Players[1]).CanPlay());
        });

        await check("Octagram stops later native repeats and retains wrapper cleanup", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Togawasakiko");
            PowerModel power = ApplyPower(f);
            CardModel card = f.State.CreateCard(ModelDb.Card<Whirlwind>(), f.Players[0]);
            card.BaseReplayCount = 2;
            card.EnergyCost.CapturedXValue = 0;
            card.EnergyCost.SetUntilPlayed(0);
            int completed = 0;
            card.ExecutionFinished += _ =>
            {
                if (CombatManager.Instance.History.CardPlaysStarted.Any(e => e.CardPlay.Card == card))
                    SetEnded(power);
            };
            card.Played += () => completed++;

            await CardCmd.AutoPlay(f.Choice, card, null, skipXCapture: true, skipCardPileVisuals: true);

            Equal(1, completed);
            Equal(1, CombatManager.Instance.History.CardPlaysStarted.Count(e => e.CardPlay.Card == card));
            Equal(1, CombatManager.Instance.History.CardPlaysFinished.Count(e => e.CardPlay.Card == card));
            Equal(PileType.Discard, card.Pile!.Type);
            Equal(false, card.EnergyCost.HasLocalModifiers);
            Equal(0, card.CurrentPlayIndex);
            Equal(null, card.CurrentTarget);
            Equal(false, CombatManager.Instance.IsExecutingCardOrPotionEffect(f.Players[0]));
        });

        await check("Octagram ended owner cannot move a new autoplay card from exhaust", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Togawasakiko");
            SetEnded(ApplyPower(f));
            CardModel card = f.State.CreateCard(ModelDb.Card<Whirlwind>(), f.Players[0]);
            await CardPileCmd.Add(card, PileType.Exhaust);
            await CardCmd.AutoPlay(f.Choice, card, null, skipCardPileVisuals: true);
            Equal(PileType.Exhaust, card.Pile!.Type);
            Equal(0, CombatManager.Instance.History.CardPlaysStarted.Count(e => e.CardPlay.Card == card));
        });

        await check("Octagram rejected autoplay cleans an already staged Play card through the native fallback", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Togawasakiko");
            SetEnded(ApplyPower(f));
            CardModel card = f.State.CreateCard(ModelDb.Card<Whirlwind>(), f.Players[0]);
            await CardPileCmd.Add(card, PileType.Play);
            await CardCmd.AutoPlay(f.Choice, card, null, skipCardPileVisuals: true);
            Equal(PileType.Discard, card.Pile!.Type);
            Equal(0, CombatManager.Instance.History.CardPlaysStarted.Count(e => e.CardPlay.Card == card));
        });

        await check("Octagram ended owner does not stop another owner's three native repeats", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Togawasakiko");
            SetEnded(ApplyPower(f));
            CardModel card = f.State.CreateCard(ModelDb.Card<Whirlwind>(), f.Players[1]);
            card.BaseReplayCount = 2;
            card.EnergyCost.CapturedXValue = 0;
            await CardCmd.AutoPlay(f.Choice, card, null, skipXCapture: true, skipCardPileVisuals: true);
            Equal(3, CombatManager.Instance.History.CardPlaysFinished.Count(e => e.CardPlay.Card == card));
            Equal(PileType.Discard, card.Pile!.Type);
        });

        await check("Octagram boundary never skips the first already-entered wrapper iteration", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Togawasakiko");
            SetEnded(ApplyPower(f));
            CardModel card = f.State.CreateCard(ModelDb.Card<Whirlwind>(), f.Players[0]);
            card.BaseReplayCount = 2;
            card.EnergyCost.CapturedXValue = 0;
            await card.OnPlayWrapper(f.Choice, null, true, default, skipCardPileVisuals: true);
            Equal(1, CombatManager.Instance.History.CardPlaysFinished.Count(e => e.CardPlay.Card == card));
            Equal(PileType.Discard, card.Pile!.Type);
        });
    }

    private static PowerModel ApplyPower(CombatFixture fixture)
    {
        PowerModel power = CombatFixture.Canonical<PowerModel>("OctagramDancePower").ToMutable();
        power.ApplyInternal(fixture.Players[0].Creature, 1, true);
        return power;
    }

    private static void SetEnded(PowerModel power)
    {
        // Isolate replay boundaries from the separate RunManager/EndTurn host lifecycle.
        object data = typeof(PowerModel).GetField("_internalData", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(power)!;
        data.GetType().GetField("Ended")!.SetValue(data, true);
    }

    private sealed class ReadinessHost : IDisposable
    {
        private readonly PropertyInfo _service = typeof(RunManager).GetProperty(nameof(RunManager.NetService))!;
        private readonly object? _previousService;
        private readonly ulong? _previousNetId = LocalContext.NetId;
        private readonly HashSet<Player> _ready = (HashSet<Player>)typeof(CombatManager)
            .GetField("_playersReadyToEndTurn", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(CombatManager.Instance)!;
        private readonly Player[] _previousReady;

        internal ReadinessHost()
        {
            _previousService = _service.GetValue(RunManager.Instance);
            _previousReady = _ready.ToArray();
            _ready.Clear();
            _service.SetValue(RunManager.Instance, DispatchProxy.Create<INetGameService, HostOnlyNetService>());
            LocalContext.NetId = 2;
        }

        public void Dispose()
        {
            _ready.Clear();
            _ready.UnionWith(_previousReady);
            LocalContext.NetId = _previousNetId;
            _service.SetValue(RunManager.Instance, _previousService);
        }
    }

    public class HostOnlyNetService : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => targetMethod?.Name switch
        {
            "get_Type" => NetGameType.Host,
            "get_NetId" => 2UL,
            _ => throw new NotSupportedException($"Readiness-only service cannot call {targetMethod}")
        };
    }

}
