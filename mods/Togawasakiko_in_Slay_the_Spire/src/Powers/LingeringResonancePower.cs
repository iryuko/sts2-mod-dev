using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Togawasakiko_in_Slay_the_Spire;

internal sealed class LingeringResonancePower : PowerModel
{
    private int _cardsExhaustedThisTurn;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreatePowerHoverTip<PressurePower>() };

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner))
        {
            AssertMutable();
            _cardsExhaustedThisTurn = 0;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (Owner?.Player == null || !Owner.IsAlive || !CombatManager.Instance.IsInProgress
            || card.Owner != Owner.Player || _cardsExhaustedThisTurn >= 2 || Amount <= 0
            || Owner.CombatState is not { } combatState)
        {
            return;
        }

        AssertMutable();
        // Reserve before awaiting native hooks, so nested exhaustion shares this quota.
        _cardsExhaustedThisTurn++;
        int pressure = Amount;
        foreach (Creature enemy in combatState.HittableEnemies.ToList())
        {
            if (enemy.IsAlive)
            {
                await ModSupport.ApplyPressure(choiceContext, enemy, pressure, Owner, null);
            }
        }
    }
}
