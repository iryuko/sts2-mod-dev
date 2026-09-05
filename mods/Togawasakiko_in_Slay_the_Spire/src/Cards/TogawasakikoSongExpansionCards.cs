using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Togawasakiko_in_Slay_the_Spire;

internal sealed class OctagramDance : TogawasakikoCard, ISongCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public OctagramDance()
        : base(4, CardType.Skill, CardRarity.Uncommon, TargetType.Self,
            ModSupport.GetNormalUncommonPortraitPath("octagram_dance.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature.CombatState == null || !Owner.Creature.IsAlive)
        {
            return;
        }

        if (ModSupport.GetPower<OctagramDancePower>(Owner.Creature) == null)
        {
            await PowerCmd.Apply<OctagramDancePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

internal sealed class Divine : TogawasakikoCard, ISongCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new EnergyVar(1), new CardsVar(2) };

    protected override IEnumerable<IHoverTip> ManualExtraHoverTips => new IHoverTip[]
    {
        EnergyHoverTip,
        ModSupport.CreateCardHoverTip<PersonaDissociation>(),
        ModSupport.CreateCardHoverTip<SocialWithdrawal>(),
        ModSupport.CreateCardHoverTip<AllYouThinkAboutIsYourself>(),
        ModSupport.CreateCardHoverTip<OverworkAnxiety>()
    };

    public Divine()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self,
            ModSupport.GetNormalUncommonPortraitPath("divine.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.PlayerCombatState == null || CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }

        CardModel? token = (await CardSelectCmd.FromHand(choiceContext, Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1),
            card => card is GeneratedPressureCard, this)).FirstOrDefault();
        if (token?.Owner != Owner || token.Pile?.Type != PileType.Hand || token.HasBeenRemovedFromState
            || !Owner.Creature.IsAlive
            || CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }

        await CardCmd.Exhaust(choiceContext, token);
        if (!Owner.Creature.IsAlive || CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }

        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

internal sealed class InYourBlueEyes : TogawasakikoCard, ISongCard
{
    private CardModel? _replayTarget;
    private bool _extraPlayPending;
    private bool _playing;

    public InYourBlueEyes()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self,
            ModSupport.GetNormalUncommonPortraitPath("in_your_blue_eyes.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_playing || Owner?.PlayerCombatState == null || CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }

        _playing = true;
        try
        {
            // Select both original sources before any replay can move or generate cards.
            CardModel? exhausted = SelectSong(PileType.Exhaust);
            CardModel? discarded = IsUpgraded ? SelectSong(PileType.Discard) : null;
            foreach (CardModel? card in new[] { exhausted, discarded })
            {
                // Earlier Songs may draw or reshuffle a selected instance. Keep the snapshot,
                // but do not re-enter Play or recover a removed/transferred card.
                if (card == null || card.Owner != Owner || card.Pile?.IsCombatPile != true
                    || card.Pile.Type == PileType.Play || card.HasBeenRemovedFromState
                    || Owner.Creature.CombatState?.ContainsCard(card) != true
                    || !Owner.Creature.IsAlive || CombatManager.Instance.IsOverOrEnding)
                {
                    continue;
                }

                _replayTarget = card;
                _extraPlayPending = true;
                await CardCmd.AutoPlay(choiceContext, card, null);
                _extraPlayPending = false;
                _replayTarget = null;
            }
        }
        finally
        {
            _playing = false;
            _extraPlayPending = false;
            _replayTarget = null;
        }
    }

    private CardModel? SelectSong(PileType pile)
    {
        var candidates = CardPile.Get(pile, Owner)!.Cards
            .Where(card => card != this && ModSupport.IsSongCard(card)
                && !card.Keywords.Contains(CardKeyword.Unplayable)
                && card is not InYourBlueEyes { _playing: true }).ToList();
        return candidates.Count == 0 ? null : Owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
    }

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount) =>
        _extraPlayPending && card == _replayTarget ? playCount + 1 : playCount;

    public override Task AfterModifyingCardPlayCount(CardModel card)
    {
        if (card == _replayTarget)
        {
            _extraPlayPending = false;
        }
        return Task.CompletedTask;
    }

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _playing = false;
        _extraPlayPending = false;
        _replayTarget = null;
    }
}

internal sealed class TheWholeBlueWorld : TogawasakikoCard, ISongCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new HpLossVar(5m), new CardsVar(2) };

    public TheWholeBlueWorld()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self,
            ModSupport.GetNormalUncommonPortraitPath("the_whole_blue_world.png"))
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature.CombatState == null || CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }

        await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this);
        if (!Owner.Creature.IsAlive || CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }

        var songs = CardFactory.GetForCombat(Owner, ModSupport.GetSongPoolCanonicals(),
            DynamicVars.Cards.IntValue, Owner.RunState.Rng.CombatCardGeneration).ToList();
        foreach (CardModel card in songs)
        {
            if (!Owner.Creature.IsAlive || CombatManager.Instance.IsOverOrEnding)
            {
                return;
            }

            if (IsUpgraded)
            {
                card.EnergyCost.SetThisCombat(0, true);
                if (card is TogawasakikoCard song && card.EnergyCost.CostsX)
                {
                    song.BlueWorldFreeXCombat = Owner.Creature.CombatState;
                }
            }
            else
            {
                card.EnergyCost.AddThisCombat(-1);
            }

            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner, CardPilePosition.Random);
        }
    }
}
