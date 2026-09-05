using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Togawasakiko_in_Slay_the_Spire;

internal sealed class OctagramDancePower : PowerModel
{
    private sealed class Data
    {
        public CardType LastType = CardType.Skill;
        public CardPlay? Violation;
        public bool Ended;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override object InitInternalData() => new Data();

    internal bool HasEnded => GetInternalData<Data>().Ended;
    internal bool GrantsFreeEnergy => !HasEnded && Owner.IsAlive
        && CombatManager.Instance.IsInProgress && Owner.CombatState?.CurrentSide == CombatSide.Player;

    public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        bool applies = card.Owner?.Creature == Owner && GrantsFreeEnergy;
        modifiedCost = applies ? 0m : originalCost;
        return applies;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature == Owner && GrantsFreeEnergy)
        {
            Data data = GetInternalData<Data>();
            if (data.LastType == cardPlay.Card.Type)
            {
                data.Violation ??= cardPlay;
            }
            data.LastType = cardPlay.Card.Type;
        }
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Data data = GetInternalData<Data>();
        if (!data.Ended && data.Violation == cardPlay && Owner.IsAlive && CombatManager.Instance.IsInProgress)
        {
            data.Ended = true;
            Flash();
            // Native per-player readiness, never await other players inside a card effect.
            PlayerCmd.EndTurn(Owner.Player!, canBackOut: false);
        }
        return Task.CompletedTask;
    }

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType) =>
        card.Owner?.Creature != Owner || !HasEnded;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}
