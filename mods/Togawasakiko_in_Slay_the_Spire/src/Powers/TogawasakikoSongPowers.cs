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

internal sealed class AveMujicaPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Title => new("powers", "AVE_MUJICA_POWER.title");

    public override LocString Description => new("powers", "AVE_MUJICA_POWER.description");

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner != player.Creature)
        {
            return;
        }

        PlayerCombatState? combatState = player.PlayerCombatState;
        if (combatState == null)
        {
            return;
        }

        for (int i = 0; i < (int)Amount; i++)
        {
            await ModSupport.GiveRandomPressureGeneratedCardToPlayer(player);

            if (combatState.DrawPile.Cards.Count == 0)
            {
                continue;
            }

            CardModel? topCard = combatState.DrawPile.Cards.FirstOrDefault();
            if (topCard == null)
            {
                continue;
            }

            if (topCard.CanPlay())
            {
                await CardPileCmd.AutoPlayFromDrawPile(choiceContext, player, 1, CardPilePosition.Top, false);
                continue;
            }

            await CardPileCmd.Draw(choiceContext, 1m, player, false);
        }
    }
}

internal abstract class SymbolPowerBase : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}

internal sealed class SymbolIPower : SymbolPowerBase
{
    public override LocString Title => new("powers", "SYMBOL_I_POWER.title");

    public override LocString Description => new("powers", "SYMBOL_I_POWER.description");
}

internal sealed class SymbolIIPower : SymbolPowerBase
{
    public override LocString Title => new("powers", "SYMBOL_II_POWER.title");

    public override LocString Description => new("powers", "SYMBOL_II_POWER.description");
}

internal sealed class SymbolIIIPower : SymbolPowerBase
{
    public override LocString Title => new("powers", "SYMBOL_III_POWER.title");

    public override LocString Description => new("powers", "SYMBOL_III_POWER.description");
}

internal sealed class SymbolIVPower : SymbolPowerBase
{
    public override LocString Title => new("powers", "SYMBOL_IV_POWER.title");

    public override LocString Description => new("powers", "SYMBOL_IV_POWER.description");

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner != player.Creature || Amount <= 0)
        {
            return;
        }

        await CardPileCmd.Draw(choiceContext, Amount, player, false);
    }
}

internal sealed class FaceReactionPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Title => new("powers", "FACE_REACTION_POWER.title");

    public override LocString Description => new("powers", "FACE_REACTION_POWER.description");

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (Owner == null || target != Owner || result.TotalDamage <= 0 || dealer == null || !dealer.IsMonster)
        {
            return;
        }

        decimal pressureAmount = Amount * 3m;
        if (pressureAmount <= 0)
        {
            return;
        }

        await ModSupport.ApplyPressure(dealer, pressureAmount, Owner, cardSource);
    }

    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner != null && Owner == player.Creature)
        {
            await PowerCmd.Remove(this);
        }
    }
}

internal sealed class MirrorFlowerWaterMoonPower : PowerModel
{
    private int _damageTakenThisTurn;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Title => new("powers", "MIRROR_FLOWER_WATER_MOON_POWER.title");

    public override LocString Description => new("powers", "MIRROR_FLOWER_WATER_MOON_POWER.description");

    public override int DisplayAmount => Math.Max(Amount - _damageTakenThisTurn, 0);

    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == player.Creature)
        {
            _damageTakenThisTurn = 0;
            InvokeDisplayAmountChanged();
        }

        return Task.CompletedTask;
    }

    public override decimal ModifyHpLostAfterOsty(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (Owner == null || target != Owner || amount <= 0m)
        {
            return amount;
        }

        int remaining = Math.Max(Amount - _damageTakenThisTurn, 0);
        if (remaining <= 0)
        {
            return 0m;
        }

        return Math.Min(amount, remaining);
    }

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (Owner == null || target != Owner)
        {
            return Task.CompletedTask;
        }

        int updatedDamageTaken = Math.Min(_damageTakenThisTurn + result.UnblockedDamage, Amount);
        if (updatedDamageTaken == _damageTakenThisTurn)
        {
            return Task.CompletedTask;
        }

        _damageTakenThisTurn = updatedDamageTaken;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }
}

internal abstract class KillKissPowerBase : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected abstract string TitleKey { get; }

    protected abstract string DescriptionKey { get; }

    protected abstract bool UseMaxHpDamage { get; }

    public override LocString Title => new("powers", TitleKey);

    public override LocString Description => new("powers", DescriptionKey);

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (Owner == null || side != CombatSide.Enemy || !Owner.IsAlive)
        {
            return;
        }

        PlayerChoiceContext? choiceContext = ModSupport.CreateHookChoiceContext(this, combatState, Owner.Player);
        if (choiceContext == null)
        {
            return;
        }

        foreach (Creature enemy in ModSupport.GetEnemyCreatures(Owner).ToList())
        {
            if (!enemy.IsAlive)
            {
                continue;
            }

            if (ModSupport.GetPressure(enemy) <= (enemy.CurrentHp / 2m))
            {
                continue;
            }

            decimal damage = UseMaxHpDamage ? enemy.MaxHp : 25m;
            await CreatureCmd.Damage(choiceContext, enemy, damage, (ValueProp)0, Owner, null);
        }
    }
}

internal sealed class KillKissPower : KillKissPowerBase
{
    protected override string TitleKey => "KILL_KISS_POWER.title";

    protected override string DescriptionKey => "KILL_KISS_POWER.description";

    protected override bool UseMaxHpDamage => false;
}

internal sealed class KillKissPlusPower : KillKissPowerBase
{
    protected override string TitleKey => "KILL_KISS_PLUS_POWER.title";

    protected override string DescriptionKey => "KILL_KISS_PLUS_POWER.description";

    protected override bool UseMaxHpDamage => true;
}

internal sealed class TheWayTemporaryDexterityLossPower : TemporaryDexterityPower
{
    protected override bool IsVisibleInternal => false;

    public override AbstractModel OriginModel => ModelDb.Card<STheWay>();

    protected override bool IsPositive => false;
}
