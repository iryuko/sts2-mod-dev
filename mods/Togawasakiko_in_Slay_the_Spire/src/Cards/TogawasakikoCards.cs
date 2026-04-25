using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Togawasakiko_in_Slay_the_Spire;

internal abstract class TogawasakikoCard : CardModel
{
    private readonly string _portraitPath;

    protected TogawasakikoCard(
        int energyCost,
        CardType type,
        CardRarity rarity,
        TargetType targetType,
        string portraitPath,
        bool shouldShowInCardLibrary = true)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        _portraitPath = portraitPath;
    }

    public override string PortraitPath => _portraitPath;

    public override string BetaPortraitPath => _portraitPath;

    public override CardPoolModel Pool => ModelDb.CardPool<TogawasakikoCardPool>();

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<TogawasakikoCardPool>();

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (IHoverTip hoverTip in ManualExtraHoverTips)
            {
                yield return hoverTip;
            }

            if (ModSupport.CardMentionsPressure(this))
            {
                yield return ModSupport.CreatePowerHoverTip<PressurePower>();
            }
        }
    }

    protected virtual IEnumerable<IHoverTip> ManualExtraHoverTips => Array.Empty<IHoverTip>();

}

internal abstract class TogawasakikoEventGrantedCard : TogawasakikoCard
{
    protected TogawasakikoEventGrantedCard(
        int energyCost,
        CardType type,
        CardRarity rarity,
        TargetType targetType,
        string portraitPath)
        : base(energyCost, type, rarity, targetType, portraitPath, shouldShowInCardLibrary: false)
    {
    }

    public override CardPoolModel Pool => ModelDb.CardPool<TogawasakikoEventGrantedCardPool>();

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<TogawasakikoEventGrantedCardPool>();
}

internal sealed class StrikeTogawasakiko : TogawasakikoCard
{
    protected override HashSet<CardTag> CanonicalTags => new()
    {
        CardTag.Strike
    };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DamageVar(6m, ValueProp.Move) };

    public StrikeTogawasakiko()
        : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, ModSupport.GetBasicPortraitPath("strike_togawasakiko.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}

internal sealed class DefendTogawasakiko : TogawasakikoCard
{
    public override bool GainsBlock => true;

    protected override HashSet<CardTag> CanonicalTags => new()
    {
        CardTag.Defend
    };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new BlockVar(5m, ValueProp.Move) };

    public DefendTogawasakiko()
        : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self, ModSupport.GetBasicPortraitPath("defend_togawasakiko.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
    }
}

internal sealed class Slander : TogawasakikoCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DamageVar(4m, ValueProp.Move) };

    public Slander()
        : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, ModSupport.GetBasicPortraitPath("slander.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        decimal totalDamage = DynamicVars.Damage.BaseValue + (ModSupport.GetPressure(cardPlay.Target) * 2m);
        await DamageCmd.Attack(totalDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 0, false));
    }
}

internal sealed class Unendurable : TogawasakikoCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[]
        {
            new BlockVar(8m, ValueProp.Move),
            new("PressureAmount", 3m)
        };

    public Unendurable()
        : base(2, CardType.Skill, CardRarity.Basic, TargetType.AnyEnemy, ModSupport.GetBasicPortraitPath("unendurable.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (Owner?.Creature == null)
        {
            return;
        }

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, false);
        await ModSupport.ApplyPressure(cardPlay.Target, DynamicVars["PressureAmount"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
        DynamicVars["PressureAmount"].UpgradeValueBy(1m);
    }
}

internal sealed class IHaveAscended : TogawasakikoCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { CardKeyword.Exhaust };

    public IHaveAscended()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.None, ModSupport.GetNormalRarePortraitPath("i_have_ascended.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null)
        {
            return;
        }

        PileType destinationPile = IsUpgraded ? PileType.Hand : PileType.Discard;
        await ModSupport.AddSpecificCardToCombatPile<Apotheosis>(Owner, destinationPile, this, IsUpgraded);
    }
}

internal sealed class Thrilled : TogawasakikoCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new EnergyVar(2) };

    public Thrilled()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None, ModSupport.GetNormalUncommonPortraitPath("thrilled.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null)
        {
            return;
        }

        Player owner = Owner;
        PlayerCombatState? combatState = owner.PlayerCombatState;
        if (combatState == null)
        {
            return;
        }

        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, owner);

        List<CardModel> songCardsInHand = combatState.Hand.Cards
            .Where(ModSupport.IsSongCard)
            .ToList();
        if (songCardsInHand.Count == 0)
        {
            return;
        }

        IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromHandForDiscard(
            choiceContext,
            owner,
            new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1),
            ModSupport.IsSongCard,
            this);

        List<CardModel> cardsToDiscard = selectedCards.ToList();
        if (cardsToDiscard.Count > 0)
        {
            await CardCmd.Discard(choiceContext, cardsToDiscard);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}

internal sealed class PerkUp : TogawasakikoCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new CardsVar(0) };

    public PerkUp()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.None, ModSupport.GetNormalCommonPortraitPath("perk_up.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null)
        {
            return;
        }

        await ModSupport.GiveRandomColorlessCardToPlayer(Owner);
        if (DynamicVars.Cards.BaseValue > 0m)
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}

internal sealed class Speak : TogawasakikoCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreateCardHoverTip<PersonaDissociation>() };

    public Speak()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.None, ModSupport.GetNormalCommonPortraitPath("speak.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null)
        {
            return;
        }

        CardModel? createdCard = await ModSupport.AddSpecificCardToCombatPile<PersonaDissociation>(Owner, PileType.Hand, this);
        createdCard?.EnergyCost.SetThisCombat(0, true);
    }

    protected override void OnUpgrade()
    {
        CardCmd.ApplyKeyword(this, new[] { CardKeyword.Innate });
    }
}

internal sealed class RestorationOfPower : TogawasakikoCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new CardsVar(1) };

    public RestorationOfPower()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.None, ModSupport.GetNormalCommonPortraitPath("restoration_of_power.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null)
        {
            return;
        }

        int cardCount = DynamicVars.Cards.IntValue;
        for (int i = 0; i < cardCount; i++)
        {
            await ModSupport.GiveRandomPressureGeneratedCardToPlayer(Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}

internal sealed class PutOnYourMask : TogawasakikoCard
{
    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[]
        {
            ModSupport.CreatePowerHoverTip<WeakPower>(),
            ModSupport.CreatePowerHoverTip<FaceReactionPower>()
        };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new("WeakAmount", 2m) };

    public PutOnYourMask()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, ModSupport.GetNormalCommonPortraitPath("put_on_your_mask.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (Owner?.Creature == null)
        {
            return;
        }

        bool alreadyHasWeak = ModSupport.GetPower<WeakPower>(cardPlay.Target)?.Amount > 0;
        await PowerCmd.Apply<WeakPower>(cardPlay.Target, DynamicVars["WeakAmount"].BaseValue, Owner.Creature, this, false);
        if (alreadyHasWeak)
        {
            await PowerCmd.Apply<FaceReactionPower>(Owner.Creature, 1m, Owner.Creature, this, false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WeakAmount"].UpgradeValueBy(1m);
    }
}

internal sealed class SeverThePast : TogawasakikoCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DamageVar(8m, ValueProp.Move) };

    public SeverThePast()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, ModSupport.GetNormalCommonPortraitPath("sever_the_past.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (Owner == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await ModSupport.ShuffleDiscardPileIntoDrawPile(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}

internal sealed class SoManyMaggots : TogawasakikoCard
{
    public SoManyMaggots()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyPlayer, ModSupport.GetNormalCommonPortraitPath("so_many_maggots.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature? target = cardPlay.Target ?? Owner?.Creature;
        ArgumentNullException.ThrowIfNull(target, nameof(cardPlay.Target));
        if (Owner?.PlayerCombatState != null && Owner.PlayerCombatState.Hand.Cards.Count > 0)
        {
            IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromHandForDiscard(
                choiceContext,
                Owner,
                new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1),
                null,
                this);

            List<CardModel> cardsToDiscard = selectedCards.ToList();
            if (cardsToDiscard.Count > 0)
            {
                await CardCmd.Discard(choiceContext, cardsToDiscard);
            }
        }

        foreach (PowerModel power in ModSupport.GetNegativePowers(target).ToList())
        {
            await PowerCmd.Remove(power);
        }
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 0, false));
    }
}

internal sealed class AnswerMe : TogawasakikoCard
{
    public AnswerMe()
        : base(2, CardType.Skill, CardRarity.Common, TargetType.AllEnemies, ModSupport.GetNormalCommonPortraitPath("answer_me.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        foreach (Creature enemy in ModSupport.GetEnemyCreatures(Owner.Creature))
        {
            if (ModSupport.GetPressure(enemy) < 5)
            {
                await ModSupport.ApplyPressure(enemy, 7m, Owner.Creature, this);
                continue;
            }

            await PowerCmd.Apply<StrengthPower>(enemy, -1m, Owner.Creature, this, false);
        }
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 1, false));
    }
}

internal sealed class Completeness : TogawasakikoCard
{
    public Completeness()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None, ModSupport.GetNormalUncommonPortraitPath("completeness.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        Player owner = Owner;
        Creature ownerCreature = owner.Creature;
        PlayerCombatState? combatState = owner.PlayerCombatState;
        if (combatState == null)
        {
            return;
        }

        int maxSelectable = combatState.Hand.Cards.Count;
        CardSelectorPrefs prefs = new(new("cards", "COMPLETENESS_SELECT_ANY.prompt"), 0, maxSelectable)
        {
            Cancelable = false,
            RequireManualConfirmation = true
        };

        IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromHandForDiscard(
            choiceContext,
            owner,
            prefs,
            null,
            this);

        List<CardModel> cardsToDiscard = selectedCards.ToList();
        int discardedCount = cardsToDiscard.Count;
        if (discardedCount > 0)
        {
            await CardCmd.DiscardAndDraw(choiceContext, cardsToDiscard, discardedCount);
        }

        int pressureAmount = discardedCount * 2;
        if (pressureAmount <= 0)
        {
            return;
        }

        foreach (Creature enemy in ModSupport.GetEnemyCreatures(ownerCreature))
        {
            await ModSupport.ApplyPressure(enemy, pressureAmount, ownerCreature, this);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}

internal sealed class SheIsRadiant : TogawasakikoCard
{
    public SheIsRadiant()
        : base(1, CardType.Power, CardRarity.Common, TargetType.None, ModSupport.GetNormalCommonPortraitPath("she_is_radiant.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        foreach (Creature enemy in ModSupport.GetEnemyCreatures(Owner.Creature))
        {
            PressurePower? pressure = ModSupport.GetPower<PressurePower>(enemy);
            if (pressure == null || pressure.Amount <= 0)
            {
                continue;
            }

            await PowerCmd.ModifyAmount(pressure, -pressure.Amount, Owner.Creature, this, true);
        }

        await PowerCmd.Apply<StrengthPower>(Owner.Creature, 3m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 0, false));
    }
}

internal sealed class Notebook : TogawasakikoCard
{
    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreatePowerHoverTip<SocialWithdrawalPower>() };

    public Notebook()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, ModSupport.GetNormalUncommonPortraitPath("notebook.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (Owner?.Creature == null)
        {
            return;
        }

        SocialWithdrawalPower? socialWithdrawal = ModSupport.GetPower<SocialWithdrawalPower>(cardPlay.Target);
        if (socialWithdrawal == null || socialWithdrawal.Amount <= 0)
        {
            return;
        }

        decimal convertedAmount = socialWithdrawal.Amount;
        await PowerCmd.ModifyAmount(socialWithdrawal, -convertedAmount, Owner.Creature, this, true);
        await ModSupport.ApplyPressure(cardPlay.Target, convertedAmount, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        CardCmd.ApplyKeyword(this, new[] { CardKeyword.Retain });
    }
}

internal sealed class LeaveItToMe : TogawasakikoCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DamageVar(11m, ValueProp.Move) };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreatePowerHoverTip<WeakPower>() };

    public LeaveItToMe()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, ModSupport.GetNormalUncommonPortraitPath("leave_it_to_me.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (Owner?.Creature == null)
        {
            return;
        }

        Creature target = cardPlay.Target;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        PressurePower? pressure = ModSupport.GetPower<PressurePower>(target);
        if (pressure != null && pressure.Amount > 0)
        {
            decimal removedAmount = decimal.Min(7m, pressure.Amount);
            await PowerCmd.ModifyAmount(pressure, -removedAmount, Owner.Creature, this, true);
        }

        await CreatureCmd.Heal(Owner.Creature, 5m, false);

        if (target.IsAlive && ModSupport.GetPressure(target) > 0)
        {
            await PowerCmd.Apply<WeakPower>(target, 1m, Owner.Creature, this, false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}

internal sealed class DawnOfDespair : TogawasakikoCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[]
        {
            new DamageVar(2m, ValueProp.Move),
            new("HitCount", 6m)
        };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreatePowerHoverTip<SakikoDespairEchoPower>() };

    public DawnOfDespair()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, ModSupport.GetNormalUncommonPortraitPath("dawn_of_despair.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (Owner?.Creature == null)
        {
            return;
        }

        Creature target = cardPlay.Target;
        for (int i = 0; i < DynamicVars["HitCount"].IntValue; i++)
        {
            if (!target.IsAlive)
            {
                break;
            }

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(target)
                .Execute(choiceContext);
        }

        if (target.IsAlive)
        {
            await PowerCmd.Apply<SakikoDespairEchoPower>(target, 1m, Owner.Creature, this, false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HitCount"].UpgradeValueBy(1m);
    }
}

internal sealed class BarkingBarkingBarking : TogawasakikoCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[]
        {
            new DamageVar(8m, ValueProp.Move),
            new("Regen", 3m)
        };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreatePowerHoverTip<RegenPower>() };

    public override CardPoolModel Pool => ModelDb.CardPool<TogawasakikoRelicGrantedCardPool>();

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<TogawasakikoRelicGrantedCardPool>();

    public BarkingBarkingBarking()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, ModSupport.GetRelicGrantedPortraitPath("barking_barking_barking.png"), shouldShowInCardLibrary: false)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (Owner?.Creature == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        await PowerCmd.Apply<RegenPower>(Owner.Creature, DynamicVars["Regen"].BaseValue, Owner.Creature, this, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["Regen"].UpgradeValueBy(1m);
    }
}

internal sealed class BailMoney : TogawasakikoCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DamageVar(8m, ValueProp.Move) };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreatePowerHoverTip<DexterityPower>() };

    public BailMoney()
        : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, ModSupport.GetNormalUncommonPortraitPath("bail_money.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (Owner?.Creature == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        await PowerCmd.Apply<DexterityPower>(cardPlay.Target, -1m, Owner.Creature, this, false);
        await PlayerCmd.LoseGold(10m, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}

internal sealed class WeightliftingChampion : TogawasakikoCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new HpLossVar(4m) };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[]
        {
            ModSupport.CreatePowerHoverTip<StrengthPower>(),
            ModSupport.CreatePowerHoverTip<DexterityPower>()
        };

    public WeightliftingChampion()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, ModSupport.GetNormalCommonPortraitPath("weightlifting_champion.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        ModSupport.LoseHp(Owner.Creature, DynamicVars.HpLoss.BaseValue);
        await PowerCmd.Apply<StrengthPower>(Owner.Creature, 1m, Owner.Creature, this, false);
        await PowerCmd.Apply<DexterityPower>(Owner.Creature, 1m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.HpLoss.UpgradeValueBy(-2m);
    }
}

internal sealed class Housewarming : TogawasakikoCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        Array.Empty<CardKeyword>();

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreateCardHoverTip<BarkingBarkingBarking>() };

    public Housewarming()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None, ModSupport.GetNormalUncommonPortraitPath("housewarming.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null)
        {
            return;
        }

        CardModel? createdCard = await ModSupport.AddSpecificCardToCombatPile<BarkingBarkingBarking>(
            Owner,
            PileType.Hand,
            this,
            IsUpgraded);
        if (createdCard == null)
        {
            return;
        }

        CardCmd.ApplyKeyword(createdCard, new[] { CardKeyword.Ethereal, CardKeyword.Exhaust });
    }

    protected override void OnUpgrade()
    {
    }
}

internal sealed class PullmanCrash : TogawasakikoCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DamageVar(49m, ValueProp.Move) };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreatePowerHoverTip<VulnerablePower>() };

    public override CardPoolModel Pool => ModelDb.CardPool<TogawasakikoRelicGrantedCardPool>();

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<TogawasakikoRelicGrantedCardPool>();

    public PullmanCrash()
        : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, ModSupport.GetRelicGrantedPortraitPath("pullman_crash.png"), shouldShowInCardLibrary: false)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (Owner?.Creature == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        if (ModSupport.GetPressure(cardPlay.Target) > 8)
        {
            await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, 1m, Owner.Creature, this, false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(12m);
    }
}

internal sealed class FinalCurtain : TogawasakikoCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DamageVar(5m, ValueProp.Move) };

    public FinalCurtain()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies, ModSupport.GetNormalRarePortraitPath("final_curtain.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        Creature ownerCreature = Owner.Creature;
        CombatState? combatState = ownerCreature.CombatState;
        if (combatState == null)
        {
            return;
        }

        int hitCount = ModSupport.GetEnemyCreatures(ownerCreature).Count();
        if (hitCount <= 0)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(combatState)
            .WithHitCount(hitCount)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}

internal sealed class BladeThroughTheHeart : TogawasakikoCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DamageVar(12m, ValueProp.Move) };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[]
        {
            ModSupport.CreatePowerHoverTip<VulnerablePower>(),
            ModSupport.CreatePowerHoverTip<WeakPower>(),
            ModSupport.CreatePowerHoverTip<DexterityPower>()
        };

    public BladeThroughTheHeart()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies, ModSupport.GetNormalRarePortraitPath("blade_through_the_heart.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        Creature ownerCreature = Owner.Creature;
        Creature[] enemies = ModSupport.GetEnemyCreatures(ownerCreature).ToArray();
        foreach (Creature enemy in enemies)
        {
            await PowerCmd.Apply<VulnerablePower>(enemy, 2m, ownerCreature, this, false);
            await PowerCmd.Apply<WeakPower>(enemy, 2m, ownerCreature, this, false);
            await PowerCmd.Apply<DexterityPower>(enemy, -1m, ownerCreature, this, false);
        }

        if (enemies.Length == 0)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(ownerCreature.CombatState!)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8m);
    }
}

internal sealed class Fragility : TogawasakikoCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { CardKeyword.Exhaust };

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[]
        {
            new BlockVar(17m, ValueProp.Move),
            new HealVar(8m)
        };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreatePowerHoverTip<FaceReactionPower>() };

    public Fragility()
        : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self, ModSupport.GetNormalUncommonPortraitPath("fragility.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, false);
        await PowerCmd.Apply<FaceReactionPower>(Owner.Creature, 1m, Owner.Creature, this, false);
        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
        DynamicVars.Heal.UpgradeValueBy(3m);
    }
}

internal abstract class ShadowOfThePastCard : TogawasakikoEventGrantedCard
{
    private const int MaxCombats = 2;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { CardKeyword.Unplayable };

    public override bool CanBeGeneratedByModifiers => false;

    public override int MaxUpgradeLevel => 0;

    public int CombatsSeen { get; set; }

    protected ShadowOfThePastCard(string portraitPath)
        : base(0, CardType.Curse, CardRarity.Event, TargetType.None, portraitPath)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
    }

    public override async Task AfterCombatEnd(MegaCrit.Sts2.Core.Rooms.CombatRoom room)
    {
        if (Owner == null)
        {
            return;
        }

        ShadowOfThePastCard? trackedCard = ResolveTrackedDeckCard(Owner);
        if (trackedCard == null)
        {
            ModSupport.LogWarn($"Shadow card {Id.Entry} could not find a matching deck instance after combat.");
            return;
        }

        trackedCard.CombatsSeen++;
        if (trackedCard.CombatsSeen < MaxCombats)
        {
            return;
        }

        try
        {
            await trackedCard.ResolveShadowReward(Owner);
            await CardPileCmd.RemoveFromDeck(trackedCard, false);
        }
        catch (Exception ex)
        {
            ModSupport.LogError($"Shadow reward resolution failed for {Id.Entry}: {ex}");
        }
    }

    private ShadowOfThePastCard? ResolveTrackedDeckCard(Player owner)
    {
        CardPile? deck = owner.Deck;
        if (deck == null)
        {
            return null;
        }

        if (deck.Cards.OfType<ShadowOfThePastCard>().FirstOrDefault(card => ReferenceEquals(card, this)) is { } exactCard)
        {
            return exactCard;
        }

        return deck.Cards
            .OfType<ShadowOfThePastCard>()
            .FirstOrDefault(card => card.Id == Id);
    }

    protected abstract Task ResolveShadowReward(Player owner);
}

internal sealed class ShadowOfThePastI : ShadowOfThePastCard
{
    public ShadowOfThePastI()
        : base(ModSupport.GetEventGrantedPortraitPath("shadow_of_the_past_i.png"))
    {
    }

    protected override async Task ResolveShadowReward(Player owner)
    {
        if (owner.Creature == null)
        {
            return;
        }

        await CreatureCmd.GainMaxHp(owner.Creature, 7m);
    }
}

internal sealed class ShadowOfThePastII : ShadowOfThePastCard
{
    public ShadowOfThePastII()
        : base(ModSupport.GetEventGrantedPortraitPath("shadow_of_the_past_ii.png"))
    {
    }

    protected override Task ResolveShadowReward(Player owner)
    {
        return ModSupport.RemoveStarterStrikeAndDefendFromDeck(owner);
    }
}

internal sealed class ShadowOfThePastIII : ShadowOfThePastCard
{
    public ShadowOfThePastIII()
        : base(ModSupport.GetEventGrantedPortraitPath("shadow_of_the_past_iii.png"))
    {
    }

    protected override Task ResolveShadowReward(Player owner)
    {
        return ModSupport.TryUpgradeDollMask(owner);
    }
}

internal sealed class Innocence : TogawasakikoCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new("SocialWithdrawalAmount", 2m) };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[]
        {
            ModSupport.CreatePowerHoverTip<InnocencePower>(),
            ModSupport.CreatePowerHoverTip<SocialWithdrawalPower>()
        };

    public Innocence()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, ModSupport.GetNormalUncommonPortraitPath("innocence.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        await PowerCmd.Apply<InnocencePower>(Owner.Creature, DynamicVars["SocialWithdrawalAmount"].BaseValue, Owner.Creature, this, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SocialWithdrawalAmount"].UpgradeValueBy(1m);
    }
}

internal abstract class GeneratedPressureCard : TogawasakikoCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { CardKeyword.Exhaust, CardKeyword.Ethereal };

    // Pressure兑换衍生牌只能由自定义逻辑显式创建，不能进入原版随机生成/transform结果链路。
    public override bool CanBeGeneratedByModifiers => false;

    public override CardPoolModel Pool => ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<TokenCardPool>();

    protected GeneratedPressureCard(int energyCost, CardType type, TargetType targetType, string portraitFileName)
        : base(energyCost, type, CardRarity.Token, targetType, ModSupport.GetGeneratedPortraitPath(portraitFileName), false)
    {
    }
}

internal sealed class PersonaDissociation : GeneratedPressureCard
{
    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreatePowerHoverTip<PersonaDissociationPower>() };

    public PersonaDissociation()
        : base(1, CardType.Skill, TargetType.AnyEnemy, "persona_dissociation.png")
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (Owner?.Creature == null)
        {
            return;
        }

        await PowerCmd.Apply<PersonaDissociationPower>(cardPlay.Target, 1m, Owner.Creature, this, false);
    }
}

internal sealed class SocialWithdrawal : GeneratedPressureCard
{
    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreatePowerHoverTip<SocialWithdrawalPower>() };

    public SocialWithdrawal()
        : base(0, CardType.Skill, TargetType.AnyEnemy, "social_withdrawal.png")
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (Owner?.Creature == null)
        {
            return;
        }

        await PowerCmd.Apply<SocialWithdrawalPower>(cardPlay.Target, 3m, Owner.Creature, this, false);
    }
}

internal sealed class AllYouThinkAboutIsYourself : GeneratedPressureCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DamageVar(9m, ValueProp.Move) };

    public AllYouThinkAboutIsYourself()
        : base(3, CardType.Attack, TargetType.AnyEnemy, "all_you_think_about_is_yourself.png")
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        await CreatureCmd.Stun(cardPlay.Target, string.Empty);
    }
}

internal sealed class OverworkAnxiety : GeneratedPressureCard
{
    public OverworkAnxiety()
        : base(0, CardType.Skill, TargetType.Self, "overwork_anxiety.png")
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null)
        {
            return;
        }

        await CardPileCmd.Draw(choiceContext, 1m, Owner, false);
    }
}
