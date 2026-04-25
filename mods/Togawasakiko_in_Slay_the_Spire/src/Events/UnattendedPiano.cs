using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace Togawasakiko_in_Slay_the_Spire;

internal sealed class UnattendedPiano : EventModel
{
    private const string LocPrefix = "UNATTENDED_PIANO";
    private const decimal LeaveHealAmount = 12m;
    private const decimal ContinueHpLoss = 6m;

    private readonly List<CardModel> _remainingShadows =
    [
        ModelDb.Card<ShadowOfThePastI>(),
        ModelDb.Card<ShadowOfThePastII>(),
        ModelDb.Card<ShadowOfThePastIII>()
    ];

    public override string LocTable => "events";

    public override LocString InitialDescription => Loc("pages.INITIAL.description");

    public override bool IsDeterministic => false;

    public override IEnumerable<string> GetAssetPaths(IRunState runState)
    {
        yield return ModSupport.GetShadowQuestionRoomEventMainPath("start.png");
        yield return ModSupport.GetShadowQuestionRoomEventMainPath("shadow_piano.png");

        string? musicPath = ModSupport.GetShadowQuestionRoomEventMusicPath();
        if (!string.IsNullOrWhiteSpace(musicPath))
        {
            yield return musicPath;
        }
    }

    public override bool IsAllowed(IRunState runState)
    {
        if (runState is not RunState concreteRunState)
        {
            return false;
        }

        if (!concreteRunState.Players.Any(player => player.Character is Togawasakiko))
        {
            return false;
        }

        return !ModSupport.HasVisitedEvent(concreteRunState, this);
    }

    public override async Task AfterEventStarted()
    {
        if (Owner?.RunState is RunState runState)
        {
            ModSupport.MarkEventVisited(runState, this);
        }

        await Task.CompletedTask;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return
        [
            CreateOption(
                LeaveAndDrinkTea,
                "pages.INITIAL.options.LEAVE.title",
                "pages.INITIAL.options.LEAVE.description",
                "LEAVE"),
            CreateOption(
                BeginPlaying,
                "pages.INITIAL.options.PLAY.title",
                "pages.INITIAL.options.PLAY.description",
                "PLAY")
        ];
    }

    private async Task LeaveAndDrinkTea()
    {
        if (Owner?.Creature != null)
        {
            await MegaCrit.Sts2.Core.Commands.CreatureCmd.Heal(Owner.Creature, LeaveHealAmount, false);
        }

        ModSupport.StopShadowQuestionRoomEventMusic();
        SetEventFinished(Loc("pages.LEAVE.description"));
    }

    private async Task BeginPlaying()
    {
        ModSupport.TrySetCurrentEventPortrait(ModSupport.GetShadowQuestionRoomEventMainPath("shadow_piano.png"));
        ModSupport.TryPlayShadowQuestionRoomEventMusic();
        await AdvancePlayingState(1);
    }

    private async Task ContinuePlaying()
    {
        if (Owner?.Creature != null)
        {
            await ModSupport.ApplyEventHpLoss(Owner.Creature, ContinueHpLoss);
        }

        await GiveRandomRemainingShadow();
        await AdvancePlayingState(NextPlayingStage());
    }

    private Task StopPlaying()
    {
        ModSupport.StopShadowQuestionRoomEventMusic();
        SetEventFinished(Loc("pages.STOP.description"));
        return Task.CompletedTask;
    }

    private Task FinalLeave()
    {
        ModSupport.StopShadowQuestionRoomEventMusic();
        SetEventFinished(Loc("pages.FINAL_EXIT.description"));
        return Task.CompletedTask;
    }

    private async Task AdvancePlayingState(int stage)
    {
        if (_remainingShadows.Count == 0)
        {
            SetEventState(
                Loc("pages.FINAL.description"),
                [
                    CreateOption(
                        FinalLeave,
                        "pages.FINAL.options.LEAVE.title",
                        "pages.FINAL.options.LEAVE.description",
                        "FINAL_LEAVE")
                ]);
            await Task.CompletedTask;
            return;
        }

        SetEventState(
            Loc($"pages.PLAY_{stage}.description"),
            [
                CreateOption(
                    ContinuePlaying,
                    $"pages.PLAY_{stage}.options.CONTINUE.title",
                    $"pages.PLAY_{stage}.options.CONTINUE.description",
                    $"PLAY_{stage}_CONTINUE",
                    doesDamage: true),
                CreateOption(
                    StopPlaying,
                    $"pages.PLAY_{stage}.options.STOP.title",
                    $"pages.PLAY_{stage}.options.STOP.description",
                    $"PLAY_{stage}_STOP")
            ]);

        await Task.CompletedTask;
    }

    private int NextPlayingStage()
    {
        return _remainingShadows.Count switch
        {
            2 => 2,
            1 => 3,
            _ => 4
        };
    }

    private async Task GiveRandomRemainingShadow()
    {
        if (Owner == null || _remainingShadows.Count == 0)
        {
            return;
        }

        int index = Owner.PlayerRng.Rewards.NextInt(_remainingShadows.Count);
        CardModel selected = _remainingShadows[index];
        _remainingShadows.RemoveAt(index);
        await CardPileCmd.AddCursesToDeck([selected], Owner);
    }

    private EventOption CreateOption(
        Func<Task> onChosen,
        string titleKey,
        string descriptionKey,
        string optionKey,
        bool doesDamage = false)
    {
        EventOption option = new EventOption(
            this,
            onChosen,
            Loc(titleKey),
            Loc(descriptionKey),
            optionKey,
            Array.Empty<IHoverTip>());

        return doesDamage ? option.ThatDoesDamage(ContinueHpLoss) : option;
    }

    private static LocString Loc(string key)
    {
        return new("events", $"{LocPrefix}.{key}");
    }
}
