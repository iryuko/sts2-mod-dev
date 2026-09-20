using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;
using Togawasakiko_in_Slay_the_Spire;

internal sealed class CombatFixture
{
    internal Player[] Players { get; }
    internal RunState Run { get; }
    internal CombatState State { get; private set; } = null!;
    internal Creature Enemy { get; private set; } = null!;
    internal PlayerChoiceContext Choice { get; } = new ThrowingPlayerChoiceContext();
    internal int PressureAmount => Enemy.Powers.FirstOrDefault(p => p.Id.Entry == "PRESSURE_POWER")?.Amount ?? 0;

    internal CombatFixture(params string[] characters)
    {
        if (characters.Length == 0) characters = ["Togawasakiko"];
        Players = characters.Select((name, i) =>
            Player.CreateForNewRun(Canonical<CharacterModel>(name), UnlockState.all, (ulong)i + 1)).ToArray();
        Run = RunState.CreateForTest(Players, seed: "SAKIKOREGRESSION");
        StartCombat();
    }

    internal static T Canonical<T>(string name) where T : AbstractModel
    {
        Type type = typeof(TogawasakikoMod).Assembly.GetType("Togawasakiko_in_Slay_the_Spire." + name)
            ?? typeof(ModelDb).Assembly.GetType("MegaCrit.Sts2.Core.Models.Characters." + name, true)!;
        return ModelDb.GetById<T>(ModelDb.GetId(type));
    }

    internal void StartCombat()
    {
        // Replace only the scene/network host lifecycle. SetUpCombat still
        // creates real fresh combat cards from the real deck.
        typeof(CombatManager).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(CombatManager.Instance, null);
        typeof(CombatManager).GetProperty(nameof(CombatManager.IsInProgress))!.SetValue(CombatManager.Instance, false);
        CombatManager.Instance.History.Clear();
        foreach (Player player in Players) player.Creature.Reset();
        State = new CombatState(runState: Run);
        foreach (Player player in Players) State.AddPlayer(player);
        Enemy = State.CreateCreature(ModelDb.Monster<TestSubject>().ToMutable(), CombatSide.Enemy, null);
        State.AddCreature(Enemy);
        CombatManager.Instance.SetUpCombat(State);
        typeof(CombatManager).GetProperty(nameof(CombatManager.IsInProgress))!.SetValue(CombatManager.Instance, true);
        Enemy.Monster!.RollMove(Players.Select(p => p.Creature));
    }

    internal CardModel Card(string name, int player = 0) => State.CreateCard(Canonical<CardModel>(name), Players[player]);
    internal static int Cost(CardModel card) => card.EnergyCost.GetWithModifiers(CostModifiers.Local);
    internal int HandCount(int player, string entry) => Players[player].PlayerCombatState!.Hand.Cards.Count(c => c.Id.Entry == entry);
    internal void Pressure(int amount) => Canonical<PowerModel>("PressurePower").ToMutable().ApplyInternal(Enemy, amount, true);

    internal Task ApplyInferiority(CardModel? source, Creature? applier = null) => PowerCmd.Apply(
        Choice, Canonical<PowerModel>("InferiorityPower").ToMutable(), Enemy, 1, applier ?? source?.Owner.Creature, source);

    internal static CardPlay Play(CardModel card, Creature? target = null, bool autoplay = false) => new()
    {
        Card = card, Target = target, ResultPile = PileType.Discard, Resources = default,
        IsAutoPlay = autoplay, PlayIndex = 0, PlayCount = 1
    };

    internal async Task Finish(CardModel card, bool autoplay = false)
    {
        CardPlay play = Play(card, autoplay: autoplay);
        await Hook.BeforeCardPlayed(State, play);
        CombatManager.Instance.History.CardPlayStarted(State, play);
        CombatManager.Instance.History.CardPlayFinished(State, play);
        await Hook.AfterCardPlayed(State, Choice, play);
    }

    internal Task PlayEffect(CardModel card) => (Task)card.GetType()
        .GetMethod("OnPlay", BindingFlags.Instance | BindingFlags.NonPublic)!
        .Invoke(card, [Choice, Play(card, Enemy)])!;
}
