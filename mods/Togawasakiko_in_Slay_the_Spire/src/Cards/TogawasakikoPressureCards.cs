using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Togawasakiko_in_Slay_the_Spire;

internal sealed class UnspokenWords : TogawasakikoCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new BlockVar(5m, ValueProp.Move) };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        ModSupport.CreateCardHoverTip<PersonaDissociation>(),
        ModSupport.CreateCardHoverTip<SocialWithdrawal>(),
        ModSupport.CreateCardHoverTip<AllYouThinkAboutIsYourself>(),
        ModSupport.CreateCardHoverTip<OverworkAnxiety>()
    };

    public UnspokenWords()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self,
            ModSupport.GetNormalUncommonPortraitPath("unspoken_words.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.PlayerCombatState == null)
        {
            return;
        }

        int maximum = Owner.PlayerCombatState.Hand.Cards.Count(card => card is GeneratedPressureCard);
        var selected = (await CardSelectCmd.FromHand(choiceContext, Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, maximum),
            card => card is GeneratedPressureCard, this)).ToList();
        // Snapshot before exhausting: native exhaust hooks may draw more tokens.
        foreach (CardModel card in selected)
        {
            await CardCmd.Exhaust(choiceContext, card);
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}

internal sealed class LingeringResonance : TogawasakikoCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DynamicVar("Power", 1m) };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(CardKeyword.Ethereal)
    };

    public LingeringResonance()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self,
            ModSupport.GetNormalUncommonPortraitPath("lingering_resonance.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.PlayerCombatState == null)
        {
            return;
        }

        await PowerCmd.Apply<LingeringResonancePower>(choiceContext, Owner.Creature,
            DynamicVars["Power"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Power"].UpgradeValueBy(1m);
    }
}

internal sealed class UntilNextAct : TogawasakikoCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new CardsVar(1) };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        ModSupport.CreateCardHoverTip<PersonaDissociation>(),
        ModSupport.CreateCardHoverTip<SocialWithdrawal>(),
        ModSupport.CreateCardHoverTip<AllYouThinkAboutIsYourself>(),
        ModSupport.CreateCardHoverTip<OverworkAnxiety>()
    };

    public UntilNextAct()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self,
            ModSupport.GetNormalUncommonPortraitPath("until_next_act.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.PlayerCombatState == null)
        {
            return;
        }

        var selected = await CardSelectCmd.FromHand(choiceContext, Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, DynamicVars.Cards.IntValue),
            card => card is GeneratedPressureCard, this);
        foreach (CardModel card in selected)
        {
            CardCmd.RemoveKeyword(card, CardKeyword.Ethereal);
            CardCmd.ApplyKeyword(card, CardKeyword.Retain);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}

internal sealed class ComposedResponse : TogawasakikoCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new BlockVar(6m, ValueProp.Move), new DynamicVar("PressureAmount", 5m) };

    public ComposedResponse()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy,
            ModSupport.GetNormalCommonPortraitPath("composed_response.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (Owner?.PlayerCombatState == null)
        {
            return;
        }

        PressurePower? pressure = ModSupport.GetPower<PressurePower>(cardPlay.Target);
        if (pressure != null && pressure.Amount > 0)
        {
            decimal removed = decimal.Min(pressure.Amount, DynamicVars["PressureAmount"].BaseValue);
            await ModSupport.ModifyPowerAmount(choiceContext, pressure, -removed, Owner.Creature, this, true);
        }

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}
