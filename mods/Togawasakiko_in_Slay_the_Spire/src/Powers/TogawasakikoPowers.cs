using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Togawasakiko_in_Slay_the_Spire;

internal sealed class PressurePower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Title => new("powers", "PRESSURE_POWER.title");

    public override LocString Description => new("powers", "PRESSURE_POWER.description");
}

internal sealed class SakikoDespairEchoPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Title => new("powers", "SAKIKO_DESPAIR_ECHO_POWER.title");

    public override LocString Description => new("powers", "SAKIKO_DESPAIR_ECHO_POWER.description");

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (Owner == null || target != Owner || result.TotalDamage <= 0 || Amount <= 0)
        {
            return;
        }

        await ModSupport.ApplyPressure(choiceContext, target, Amount * 3m, dealer, cardSource);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
        {
            await PowerCmd.Remove(this);
        }
    }
}

internal sealed class PersonaDissociationPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Title => new("powers", "PERSONA_DISSOCIATION_POWER.title");

    public override LocString Description => new("powers", "PERSONA_DISSOCIATION_POWER.description");

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (Owner != null && target == Owner && Amount > 0 && amount > 0)
        {
            return 2m;
        }

        return 1m;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (Owner != null && target == Owner && Amount > 0 && result.TotalDamage > 0)
        {
            await PowerCmd.Decrement(this);
        }
    }
}

internal sealed class MagneticForceHellWargodPower : PowerModel
{
    private HashSet<CardModel>? _cardsQueuedForReplay;

    private HashSet<CardModel> CardsQueuedForReplay
    {
        get
        {
            AssertMutable();
            return _cardsQueuedForReplay ??= new HashSet<CardModel>();
        }
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override LocString Title => new("powers", "MAGNETIC_FORCE_HELL_WARGOD_POWER.title");

    public override LocString Description => new("powers", "MAGNETIC_FORCE_HELL_WARGOD_POWER.description");

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player == null || cardPlay.Card?.Owner != Owner.Player || cardPlay.Card.Type != CardType.Attack)
        {
            return;
        }

        CardModel card = cardPlay.Card;
        if (CardsQueuedForReplay.Remove(card))
        {
            return;
        }

        if (!ModSupport.TryResolveReplayTarget(cardPlay, out Creature? replayTarget))
        {
            return;
        }

        CardsQueuedForReplay.Add(card);
        try
        {
            await CardCmd.AutoPlay(
                choiceContext,
                card,
                replayTarget,
                AutoPlayType.Default,
                false,
                false);
        }
        finally
        {
            CardsQueuedForReplay.Remove(card);
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (Owner != null && side == Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }
}

internal sealed class SocialWithdrawalPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Title => new("powers", "SOCIAL_WITHDRAWAL_POWER.title");

    public override LocString Description => new("powers", "SOCIAL_WITHDRAWAL_POWER.description");

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (Owner == null || side != Owner.Side || !Owner.IsAlive)
        {
            return;
        }

        await CreatureCmd.Damage(choiceContext, Owner, 7m, (ValueProp)0, Owner, null);
        await PowerCmd.Decrement(this);
    }
}

internal sealed class InnocencePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Title => new("powers", "INNOCENCE_POWER.title");

    public override LocString Description => new("powers", "INNOCENCE_POWER.description");

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == null || Owner != player.Creature || Amount <= 0)
        {
            return;
        }

        foreach (Creature enemy in ModSupport.GetEnemyCreatures(Owner))
        {
            await ModSupport.ApplyPower<SocialWithdrawalPower>(choiceContext, enemy, Amount, Owner, null, false);
        }
    }
}

internal sealed class InferiorityPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Title => new("powers", "INFERIORITY_POWER.title");

    public override LocString Description => new("powers", "INFERIORITY_POWER.description");

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (Owner == null || target != Owner || result.TotalDamage <= 0)
        {
            return;
        }

        await ModSupport.ApplyPower<StrengthPower>(choiceContext, Owner, -1m, dealer, cardSource, false);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (Owner != null && side == Owner.Side)
        {
            await PowerCmd.Decrement(this);
        }
    }
}

internal sealed class TogawasakikoCombatWatcherPower : PowerModel
{
    private CardModel? _lastPlayedCardThisTurn;

    protected override bool IsVisibleInternal => false;

    public CardModel? LastPlayedCardThisTurn => _lastPlayedCardThisTurn;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override LocString Title => new("powers", "TOGAWASAKIKO_COMBAT_WATCHER_POWER.title");

    public override LocString Description => new("powers", "TOGAWASAKIKO_COMBAT_WATCHER_POWER.description");

    internal void ResetCombatState()
    {
        _lastPlayedCardThisTurn = null;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player == null || cardPlay.Card?.Owner != Owner.Player)
        {
            return Task.CompletedTask;
        }

        _lastPlayedCardThisTurn = cardPlay.Card;

        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == player.Creature)
        {
            _lastPlayedCardThisTurn = null;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power.Owner == null || !power.Owner.IsMonster || !power.Owner.IsAlive || amount == 0)
        {
            return;
        }

        Player? recipient = Owner?.Player;
        if (recipient == null || !IsPressureRedemptionOwner(recipient, applier, cardSource))
        {
            return;
        }

        if (power is WeakPower && amount > 0 && await ModSupport.TryConsumePressure(choiceContext, power.Owner, 2, applier, cardSource))
        {
            await ModSupport.GiveGeneratedCardToPlayer<PersonaDissociation>(recipient);
            return;
        }

        if (power is VulnerablePower && amount > 0 && await ModSupport.TryConsumePressure(choiceContext, power.Owner, 3, applier, cardSource))
        {
            await ModSupport.GiveGeneratedCardToPlayer<AllYouThinkAboutIsYourself>(recipient);
            return;
        }

        if ((power is StrengthPower or DexterityPower) && amount < 0 && await ModSupport.TryConsumePressure(choiceContext, power.Owner, 1, applier, cardSource))
        {
            await ModSupport.GiveGeneratedCardToPlayer<SocialWithdrawal>(recipient);
            return;
        }

    }

    private static bool IsPressureRedemptionOwner(
        Player watcherPlayer,
        Creature? applier,
        CardModel? cardSource)
    {
        if (cardSource?.Owner?.Character is Togawasakiko)
        {
            return cardSource.Owner == watcherPlayer;
        }

        if (applier?.Player?.Character is Togawasakiko)
        {
            return applier.Player == watcherPlayer;
        }

        return watcherPlayer.Creature?.CombatState?.Players
            .FirstOrDefault(player =>
                player.Character is Togawasakiko && player.Creature?.IsAlive == true) == watcherPlayer;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (Owner == null || side != Owner.Side || !Owner.IsAlive)
        {
            return;
        }

        int symbolKinds = ModSupport.CountOwnedSymbolKinds(Owner);
        if (symbolKinds >= 2)
        {
            await CreatureCmd.GainBlock(Owner, 5m, (ValueProp)0, null!, false);
        }

        if (symbolKinds >= 3)
        {
            Creature? randomEnemy = ModSupport.GetRandomEnemy(Owner);
            if (randomEnemy != null)
            {
                await CreatureCmd.Damage(choiceContext, randomEnemy, 13m, (ValueProp)0, Owner, null);
            }
        }

        if (symbolKinds >= 4)
        {
            foreach (Creature enemy in ModSupport.GetEnemyCreatures(Owner))
            {
                await ModSupport.ApplyPressure(choiceContext, enemy, 10m, Owner, null);
            }
        }

        await ModSupport.RemoveTemporarySymbolPowers(choiceContext, Owner);
    }
}
