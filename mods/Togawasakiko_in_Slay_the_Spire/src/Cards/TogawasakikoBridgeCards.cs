using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Togawasakiko_in_Slay_the_Spire;

// Approved bridge cards are not Songs.
internal sealed class UnfinishedScore : TogawasakikoCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new BlockVar(5m, ValueProp.Move) };

    public UnfinishedScore()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self,
            ModSupport.GetNormalCommonPortraitPath("unfinished_score.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.PlayerCombatState == null)
        {
            return;
        }

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        CardModel? song = (await CardSelectCmd.FromCombatPile(
            choiceContext, PileType.Discard.GetPile(Owner), Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1), ModSupport.IsSongCard)).FirstOrDefault();
        if (song != null)
        {
            await CardPileCmd.Add(song, PileType.Draw, CardPilePosition.Top);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}

internal sealed class FollowingPhrase : TogawasakikoCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new BlockVar(6m, ValueProp.Move), new CardsVar(1) };

    protected override bool ShouldGlowGoldInternal => FollowsSong();

    public FollowingPhrase()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self,
            ModSupport.GetNormalCommonPortraitPath("following_phrase.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.PlayerCombatState == null)
        {
            return;
        }

        bool followsSong = FollowsSong(cardPlay);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        if (followsSong)
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }

    private bool FollowsSong(CardPlay? currentPlay = null)
    {
        if (Owner?.Creature.CombatState is not { } combatState)
        {
            return false;
        }

        // Started history preserves nesting order. Exclude this play, not every
        // play of this card instance, so native replays do not reuse a prior Song.
        CardModel? previous = CombatManager.Instance.History.CardPlaysStarted
            .LastOrDefault(entry => entry.HappenedThisTurn(combatState)
                && entry.CardPlay.Card.Owner == Owner
                && !ReferenceEquals(entry.CardPlay, currentPlay))?.CardPlay.Card;
        return previous != null && ModSupport.IsSongCard(previous);
    }
}

internal sealed class Unmask : TogawasakikoCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new CardsVar(1), new DynamicVar("PressureAmount", 3m) };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips =>
        new IHoverTip[] { ModSupport.CreatePowerHoverTip<FaceReactionPower>() };

    public Unmask()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy,
            ModSupport.GetNormalUncommonPortraitPath("unmask.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        if (Owner?.PlayerCombatState == null)
        {
            return;
        }

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        FaceReactionPower? face = ModSupport.GetPower<FaceReactionPower>(Owner.Creature);
        if (face == null || face.Amount <= 0)
        {
            return;
        }

        await PowerCmd.Remove(face);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        if (cardPlay.Target.IsAlive)
        {
            await ModSupport.ApplyPressure(choiceContext, cardPlay.Target,
                DynamicVars["PressureAmount"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PressureAmount"].UpgradeValueBy(2m);
    }
}

internal sealed class RehearsalOrder : TogawasakikoCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public RehearsalOrder()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self,
            ModSupport.GetNormalUncommonPortraitPath("rehearsal_order.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.PlayerCombatState == null)
        {
            return;
        }

        CardModel? song = (await CardSelectCmd.FromCombatPile(
            choiceContext, PileType.Draw.GetPile(Owner), Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1),
            card => card.Rarity is CardRarity.Common or CardRarity.Uncommon && ModSupport.IsSongCard(card))).FirstOrDefault();
        if (song != null)
        {
            await CardPileCmd.Add(song, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

internal sealed class BackstageSupport : TogawasakikoCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DynamicVar("Power", 3m) };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.Static(StaticHoverTip.Block),
        ModSupport.CreateCardHoverTip<PersonaDissociation>(),
        ModSupport.CreateCardHoverTip<SocialWithdrawal>(),
        ModSupport.CreateCardHoverTip<AllYouThinkAboutIsYourself>(),
        ModSupport.CreateCardHoverTip<OverworkAnxiety>()
    };

    public BackstageSupport()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self,
            ModSupport.GetNormalUncommonPortraitPath("backstage_support.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.PlayerCombatState == null)
        {
            return;
        }

        await PowerCmd.Apply<BackstageSupportPower>(choiceContext, Owner.Creature,
            DynamicVars["Power"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Power"].UpgradeValueBy(1m);
    }
}
