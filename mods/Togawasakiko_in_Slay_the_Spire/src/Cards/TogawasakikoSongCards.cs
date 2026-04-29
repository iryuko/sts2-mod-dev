using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Togawasakiko_in_Slay_the_Spire;

internal interface ISongCard
{
}

internal sealed class Compose : TogawasakikoCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { CardKeyword.Exhaust };

    public Compose()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, ModSupport.GetNormalUncommonPortraitPath("compose.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null)
        {
            return;
        }

        await ModSupport.GiveRandomSongCardToPlayer(Owner, true);
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 0, false));
    }
}

internal sealed class AveMujica : TogawasakikoCard, ISongCard
{
    public AveMujica()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, ModSupport.GetNormalRarePortraitPath("ave_mujica.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        await PowerCmd.Apply<AveMujicaPower>(Owner.Creature, 1m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 2, false));
    }
}

internal sealed class AWonderfulWorldYetNowhereToBeFound : TogawasakikoCard, ISongCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new("DamageCap", 20m) };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreatePowerHoverTip<MirrorFlowerWaterMoonPower>() };

    public AWonderfulWorldYetNowhereToBeFound()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, ModSupport.GetNormalRarePortraitPath("a_wonderful_world_yet_nowhere_to_be_found.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        decimal damageCap = DynamicVars["DamageCap"].BaseValue;
        MirrorFlowerWaterMoonPower? existingPower = ModSupport.GetPower<MirrorFlowerWaterMoonPower>(Owner.Creature);
        if (existingPower != null)
        {
            if (existingPower.Amount == damageCap)
            {
                return;
            }

            await PowerCmd.Remove(existingPower);
        }

        await PowerCmd.Apply<MirrorFlowerWaterMoonPower>(Owner.Creature, damageCap, Owner.Creature, this, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DamageCap"].UpgradeValueBy(-3m);
    }
}

internal sealed class Angles : TogawasakikoCard, ISongCard
{
    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[]
        {
            ModSupport.CreatePowerHoverTip<InferiorityPower>(),
            ModSupport.CreatePowerHoverTip<VulnerablePower>()
        };

    protected override bool ShouldGlowGoldInternal => ModSupport.AllLivingEnemiesWithIntentAttack(Owner?.Creature);

    public Angles()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies, ModSupport.GetNormalUncommonPortraitPath("angles.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        Creature ownerCreature = Owner.Creature;
        List<Creature> livingEnemies = ModSupport.GetEnemyCreatures(ownerCreature).Where(enemy => enemy.IsAlive).ToList();
        if (livingEnemies.Count == 0)
        {
            return;
        }

        List<Creature> enemiesWithIntent = ModSupport.GetLivingEnemiesWithIntent(ownerCreature).ToList();
        if (enemiesWithIntent.Count == 0)
        {
            return;
        }

        bool shouldApplyInferiority = enemiesWithIntent.All(ModSupport.DoesCurrentMoveContainAttackIntent);
        foreach (Creature enemy in livingEnemies)
        {
            if (shouldApplyInferiority)
            {
                await PowerCmd.Apply<InferiorityPower>(enemy, 1m, ownerCreature, this, false);
                continue;
            }

            await PowerCmd.Apply<VulnerablePower>(enemy, 1m, ownerCreature, this, false);
        }
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 0, false));
    }
}

internal sealed class Ether : TogawasakikoCard, ISongCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[]
        {
            new DamageVar(5m, ValueProp.Move),
            new("HitCount", 2m),
            new("PressureAmount", 2m)
        };

    public Ether()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, ModSupport.GetNormalUncommonPortraitPath("ether.png"))
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
        Creature ownerCreature = Owner.Creature;
        int hitCount = DynamicVars["HitCount"].IntValue;

        for (int i = 0; i < hitCount; i++)
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

        if (!target.IsAlive)
        {
            return;
        }

        decimal pressureAmount = DynamicVars["PressureAmount"].BaseValue;
        await ModSupport.ApplyPressure(target, pressureAmount, ownerCreature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HitCount"].UpgradeValueBy(1m);
        DynamicVars["PressureAmount"].UpgradeValueBy(1m);
    }
}

internal sealed class GeorgetteMeGeorgetteYou : TogawasakikoCard, ISongCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[]
        {
            new DamageVar(7m, ValueProp.Move),
            new("PressureAmount", 7m)
        };

    public GeorgetteMeGeorgetteYou()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, ModSupport.GetNormalUncommonPortraitPath("georgette_me_georgette_you.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (Owner?.Creature == null)
        {
            return;
        }

        Creature ownerCreature = Owner.Creature;
        Creature target = cardPlay.Target;
        if (ownerCreature.CurrentHp >= target.CurrentHp)
        {
            await ModSupport.ApplyPressure(target, DynamicVars["PressureAmount"].BaseValue, ownerCreature, this);
            return;
        }

        await CreatureCmd.Damage(choiceContext, target, DynamicVars.Damage.BaseValue, (ValueProp)0, ownerCreature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["PressureAmount"].UpgradeValueBy(2m);
    }
}

internal sealed class SymbolI : TogawasakikoCard, ISongCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DamageVar(3m, ValueProp.Move) };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreatePowerHoverTip<SymbolIPower>() };

    public SymbolI()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy, ModSupport.GetNormalCommonPortraitPath("symbol_i.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        Creature ownerCreature = Owner.Creature;
        for (int i = 0; i < 3; i++)
        {
            Creature? randomEnemy = ModSupport.GetRandomEnemy(ownerCreature);
            if (randomEnemy == null)
            {
                break;
            }

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(randomEnemy)
                .Execute(choiceContext);
        }

        await PowerCmd.Apply<SymbolIPower>(ownerCreature, 1m, ownerCreature, this, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }
}

internal sealed class SymbolIi : TogawasakikoCard, ISongCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DamageVar(6m, ValueProp.Move) };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreatePowerHoverTip<SymbolIIPower>() };

    public SymbolIi()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies, ModSupport.GetNormalUncommonPortraitPath("symbol_ii.png"))
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

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(combatState)
            .Execute(choiceContext);

        foreach (Creature enemy in ModSupport.GetEnemyCreatures(ownerCreature).Where(enemy => enemy.IsAlive))
        {
            await PowerCmd.Apply<InferiorityPower>(enemy, 1m, ownerCreature, this, false);
            await ModSupport.TryGenerateInferiorityPressureCard(enemy, ownerCreature, this);
        }

        await PowerCmd.Apply<SymbolIIPower>(ownerCreature, 1m, ownerCreature, this, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}

internal sealed class SymbolIii : TogawasakikoCard, ISongCard
{
    private int _currentBlock = 3;
    private int _increasedBlock;

    public override bool GainsBlock => true;

    [SavedProperty]
    public int CurrentBlock
    {
        get => _currentBlock;
        set
        {
            AssertMutable();
            _currentBlock = value;
            DynamicVars.Block.BaseValue = _currentBlock;
        }
    }

    [SavedProperty]
    public int IncreasedBlock
    {
        get => _increasedBlock;
        set
        {
            AssertMutable();
            _increasedBlock = value;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[]
        {
            new BlockVar(CurrentBlock, ValueProp.Move),
            new("Increase", 1m)
        };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreatePowerHoverTip<SymbolIIIPower>() };

    public SymbolIii()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, ModSupport.GetNormalUncommonPortraitPath("symbol_iii.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, false);
        int extraBlock = DynamicVars["Increase"].IntValue;
        BuffFromPlay(extraBlock);
        (DeckVersion as SymbolIii)?.BuffFromPlay(extraBlock);
        await PowerCmd.Apply<SymbolIIIPower>(Owner.Creature, 1m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
        UpdateBlock();
    }

    protected override void AfterDowngraded()
    {
        UpdateBlock();
    }

    private void BuffFromPlay(int extraBlock)
    {
        IncreasedBlock += extraBlock;
        UpdateBlock();
    }

    private void UpdateBlock()
    {
        CurrentBlock = (IsUpgraded ? 7 : 3) + IncreasedBlock;
    }
}

internal sealed class SymbolIv : TogawasakikoCard, ISongCard
{
    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreatePowerHoverTip<SymbolIVPower>() };

    public SymbolIv()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self, ModSupport.GetNormalRarePortraitPath("symbol_iv.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        await PowerCmd.Apply<SymbolIVPower>(Owner.Creature, 1m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 1, false));
    }
}

internal sealed class CrucifixX : TogawasakikoCard, ISongCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DamageVar(6m, ValueProp.Move) };

    public CrucifixX()
        : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies, ModSupport.GetNormalUncommonPortraitPath("crucifix_x.png"))
    {
        MockSetEnergyCost(new CardEnergyCost(this, 0, true));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        int xValue = ResolveEnergyXValue();
        int extraHits = ModSupport.DoAllLivingEnemiesHavePressure(Owner.Creature, 4) ? 2 : 0;
        int totalHits = xValue + extraHits;
        if (totalHits <= 0)
        {
            return;
        }

        Creature ownerCreature = Owner.Creature;
        CombatState? combatState = ownerCreature.CombatState;
        if (combatState == null)
        {
            return;
        }

        List<Creature> livingEnemies = ModSupport.GetEnemyCreatures(ownerCreature)
            .Where(enemy => enemy.IsAlive)
            .ToList();
        if (livingEnemies.Count == 0)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(combatState)
            .WithHitCount(totalHits)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }
}

internal sealed class Face : TogawasakikoCard, ISongCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new BlockVar(8m, ValueProp.Move) };

    public Face()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, ModSupport.GetNormalCommonPortraitPath("face.png"))
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
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
    }
}

internal sealed class SakiMovePlz : TogawasakikoCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DamageVar(9m, ValueProp.Move) };

    protected override bool ShouldGlowGoldInternal => ShouldApplyVulnerable();

    public SakiMovePlz()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, ModSupport.GetNormalCommonPortraitPath("saki_move_plz.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        bool shouldApplyVulnerable = ShouldApplyVulnerable();

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        if (!shouldApplyVulnerable || !cardPlay.Target.IsAlive || Owner?.Creature == null)
        {
            return;
        }

        await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, 1m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }

    private bool ShouldApplyVulnerable()
    {
        if (Owner == null)
        {
            return false;
        }

        CardModel? previousCard = ModSupport.GetPreviouslyPlayedCardThisTurn(Owner, this);
        return previousCard != null && ModSupport.IsSongCard(previousCard);
    }
}

internal sealed class MusicOfTheCelestialSphere : TogawasakikoCard, ISongCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new("PressureDivisor", 5m) };

    public MusicOfTheCelestialSphere()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies, ModSupport.GetNormalUncommonPortraitPath("music_of_the_celestial_sphere.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        int divisor = DynamicVars["PressureDivisor"].IntValue;
        foreach (Creature enemy in ModSupport.GetEnemyCreatures(Owner.Creature))
        {
            int strengthLoss = ModSupport.GetPressure(enemy) / divisor;
            if (strengthLoss <= 0)
            {
                continue;
            }

            await PowerCmd.Apply<StrengthPower>(enemy, -strengthLoss, Owner.Creature, this, false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PressureDivisor"].UpgradeValueBy(-1m);
    }
}

internal sealed class KillKiss : TogawasakikoCard, ISongCard
{
    public KillKiss()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, ModSupport.GetNormalRarePortraitPath("killkiss.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        KillKissPower? normalPower = ModSupport.GetPower<KillKissPower>(Owner.Creature);
        KillKissPlusPower? upgradedPower = ModSupport.GetPower<KillKissPlusPower>(Owner.Creature);

        if (IsUpgraded)
        {
            if (normalPower != null)
            {
                await PowerCmd.Remove(normalPower);
            }

            if (upgradedPower == null)
            {
                await PowerCmd.Apply<KillKissPlusPower>(Owner.Creature, 1m, Owner.Creature, this, false);
            }

            return;
        }

        if (upgradedPower != null)
        {
            return;
        }

        if (normalPower == null)
        {
            await PowerCmd.Apply<KillKissPower>(Owner.Creature, 1m, Owner.Creature, this, false);
        }
    }
}

internal sealed class BlackBirthday : TogawasakikoCard, ISongCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[]
        {
            new EnergyVar(1),
            new EnergyVar("BonusEnergy", 2)
        };

    public BlackBirthday()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, ModSupport.GetNormalUncommonPortraitPath("black_birthday.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (Owner == null)
        {
            return;
        }

        await PlayerCmd.GainEnergy(1m, Owner);
        if (ModSupport.GetPressure(cardPlay.Target) > 5)
        {
            await PlayerCmd.GainEnergy(2m, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 0, false));
    }
}

internal sealed class TreasurePleasure : TogawasakikoCard, ISongCard
{
    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[]
        {
            ModSupport.CreatePowerHoverTip<PersonaDissociationPower>(),
            ModSupport.CreatePowerHoverTip<MagneticForceHellWargodPower>()
        };

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { CardKeyword.Exhaust };

    public TreasurePleasure()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None, ModSupport.GetNormalUncommonPortraitPath("treasure_pleasure.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        await PowerCmd.Apply<PersonaDissociationPower>(Owner.Creature, 1m, Owner.Creature, this, false);
        await PowerCmd.Apply<MagneticForceHellWargodPower>(Owner.Creature, 1m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

internal sealed class ChoirSChoir : TogawasakikoCard, ISongCard
{
    public ChoirSChoir()
        : base(3, CardType.Skill, CardRarity.Rare, TargetType.None, ModSupport.GetNormalRarePortraitPath("choir_s_choir.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null)
        {
            return;
        }

        PlayerCombatState? combatState = Owner.PlayerCombatState;
        if (combatState != null && combatState.Hand.Cards.Count > 0)
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

        PlayerCombatState? ownerCombatState = Owner.PlayerCombatState;
        if (ownerCombatState == null)
        {
            return;
        }

        List<CardModel> exhaustCards = ownerCombatState.ExhaustPile.Cards.ToList();
        foreach (CardModel card in exhaustCards)
        {
            card.EnergyCost.SetUntilPlayed(0, true);
            if (!ModSupport.TryResolveAutoplayTarget(card, out Creature? autoplayTarget))
            {
                ModSupport.LogWarn($"ChoirSChoir skipped autoplay for {card.Id.Entry} because no safe autoplay target could be resolved.");
                continue;
            }

            await CardCmd.AutoPlay(
                choiceContext,
                card,
                autoplayTarget,
                AutoPlayType.Default,
                false,
                false);
        }
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 2, false));
    }
}

internal sealed class ImprisonedXii : TogawasakikoCard, ISongCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { CardKeyword.Unplayable };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new CardsVar(1) };

    public ImprisonedXii()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None, ModSupport.GetNormalUncommonPortraitPath("imprisoned_xii.png"))
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        await base.AfterCardChangedPiles(card, oldPileType, source);

        if (!ReferenceEquals(card, this) || Owner == null || oldPileType == PileType.Hand || Pile?.Type != PileType.Hand)
        {
            return;
        }

        PlayerChoiceContext? choiceContext = ModSupport.CreateBestEffortCombatChoiceContext(this, Owner);
        if (choiceContext == null)
        {
            return;
        }

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}

internal sealed class GodYouFool : TogawasakikoCard, ISongCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new HpLossVar(3m) };

    public GodYouFool()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.None, ModSupport.GetNormalCommonPortraitPath("god_you_fool.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null)
        {
            return;
        }

        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature,
            DynamicVars.HpLoss.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            this);
        await CardPileCmd.Draw(choiceContext, 2m, Owner, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.HpLoss.UpgradeValueBy(-2m);
    }
}

internal sealed class MasqueradeRhapsodyRequest : TogawasakikoCard, ISongCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { CardKeyword.Exhaust };

    public MasqueradeRhapsodyRequest()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, ModSupport.GetNormalUncommonPortraitPath("masquerade_rhapsody_request.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (Owner?.Creature == null)
        {
            return;
        }

        int missingHp = ModSupport.GetLostHp(Owner.Creature);
        int pressureToApply = missingHp / 2;
        if (pressureToApply <= 0)
        {
            return;
        }

        await ModSupport.ApplyPressure(cardPlay.Target, pressureToApply, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

internal sealed class STheWay : TogawasakikoCard, ISongCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new DamageVar(8m, ValueProp.Move), new HpLossVar(4m) };

    public STheWay()
        : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, ModSupport.GetNormalCommonPortraitPath("s_the_way.png"))
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

        if (!target.IsAlive)
        {
            await CardPileCmd.Draw(choiceContext, 1m, Owner, false);
            return;
        }

        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature,
            DynamicVars.HpLoss.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            this);
        await PowerCmd.Apply<TheWayTemporaryDexterityLossPower>(Owner.Creature, 1m, Owner.Creature, this, false);
        await PowerCmd.Apply<TheWayTemporaryDexterityLossPower>(target, 1m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);
    }
}

internal sealed class TwoMoonsDeepIntoTheForest : TogawasakikoCard, ISongCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new DamageVar(20m, ValueProp.Move), new EnergyVar(1) };

    public TwoMoonsDeepIntoTheForest()
        : base(7, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, ModSupport.GetNormalUncommonPortraitPath("two_moons_deep_into_the_forest.png"))
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

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (!ReferenceEquals(card, this))
        {
            return Task.CompletedTask;
        }

        ModSupport.ClearLocalCostModifiers(this);

        if (IsClone)
        {
            return Task.CompletedTask;
        }

        int songCardsPlayedThisCombat = 0;
        foreach (CardPlayFinishedEntry entry in CombatManager.Instance.History.CardPlaysFinished)
        {
            CardModel playedCard = entry.CardPlay.Card;
            if (playedCard.Owner == Owner && ModSupport.IsSongCard(playedCard))
            {
                songCardsPlayedThisCombat++;
            }
        }

        AddThisCombatSongDiscount(songCardsPlayedThisCombat);
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel playedCard = cardPlay.Card;
        if (playedCard.Owner != Owner || !ModSupport.IsSongCard(playedCard))
        {
            return Task.CompletedTask;
        }

        AddThisCombatSongDiscount(1);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(10m);
    }

    private void AddThisCombatSongDiscount(int songCount)
    {
        if (songCount <= 0)
        {
            return;
        }

        EnergyCost.AddThisCombat(-songCount * DynamicVars.Energy.IntValue, false);
    }
}

internal sealed class Sophie : TogawasakikoCard, ISongCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreatePowerHoverTip<InferiorityPower>() };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new BlockVar(7m, ValueProp.Move) };

    public Sophie()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, ModSupport.GetNormalUncommonPortraitPath("sophie.png"))
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
        await PowerCmd.Apply<InferiorityPower>(cardPlay.Target, 1m, Owner.Creature, this, false);
        await ModSupport.TryGenerateInferiorityPressureCard(cardPlay.Target, Owner.Creature, this);
        await PowerCmd.Apply<WeakPower>(Owner.Creature, 1m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(5m);
    }
}
