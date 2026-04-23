using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace Togawasakiko_in_Slay_the_Spire;

internal sealed class Togawasakiko : CharacterModel
{
    protected override string CharacterSelectIconPath => "res://images/packed/character_select/char_select_togawasakiko.png";

    protected override string CharacterSelectLockedIconPath => "res://images/packed/character_select/char_select_togawasakiko_locked.png";

    protected override string MapMarkerPath => "res://images/ui/top_panel/character_icon_togawasakiko.png";

    protected override IEnumerable<string> ExtraAssetPaths => new[]
    {
        "res://images/ui/top_panel/character_icon_togawasakiko_outline.png",
        "res://images/ui/hands/multiplayer_hand_togawasakiko_point.png",
        "res://images/ui/hands/multiplayer_hand_togawasakiko_rock.png",
        "res://images/ui/hands/multiplayer_hand_togawasakiko_paper.png",
        "res://images/ui/hands/multiplayer_hand_togawasakiko_scissors.png",
        "res://images/packed/sprite_fonts/togawasakiko_energy_icon.png",
        "res://images/packed/character/merchant/merchant_portrait_togawasakiko.png",
        "res://images/packed/character/rest_site/rest_site_character_togawasakiko.png",
        "res://scenes/ui/character_icons/togawasakiko_icon.tscn",
        "res://scenes/screens/char_select/char_select_bg_togawasakiko.tscn",
        "res://scenes/combat/energy_counters/togawasakiko_energy_counter.tscn",
        "res://scenes/creature_visuals/togawasakiko.tscn",
        "res://scenes/merchant/characters/togawasakiko_merchant.tscn",
        "res://scenes/rest_site/characters/togawasakiko_rest_site.tscn"
    };

    public override float AttackAnimDelay => 0.25f;

    public override CardPoolModel CardPool => ModelDb.CardPool<TogawasakikoCardPool>();

    public override float CastAnimDelay => 0.25f;

    public override string CharacterSelectSfx => "event:/sfx/characters/silent/silent_select";

    public override CharacterGender Gender => CharacterGender.Feminine;

    public override Color NameColor => new(0.95f, 0.84f, 0.74f, 1f);

    public override Color DialogueColor => new(0.95f, 0.84f, 0.74f, 1f);

    public override Color MapDrawingColor => new(0.55f, 0.19f, 0.18f, 1f);

    public override Color RemoteTargetingLineColor => new(0.78f, 0.40f, 0.36f, 1f);

    public override Color RemoteTargetingLineOutline => new(0.18f, 0.05f, 0.05f, 1f);

    public override Color EnergyLabelOutlineColor => new(0.18f, 0.05f, 0.05f, 1f);

    public override PotionPoolModel PotionPool => ModelDb.PotionPool<TogawasakikoPotionPool>();

    public override RelicPoolModel RelicPool => ModelDb.RelicPool<TogawasakikoRelicPool>();

    public override IEnumerable<CardModel> StartingDeck
    {
        get
        {
            foreach (int _ in Enumerable.Range(0, 4))
            {
                yield return ModelDb.Card<StrikeTogawasakiko>();
            }

            foreach (int _ in Enumerable.Range(0, 4))
            {
                yield return ModelDb.Card<DefendTogawasakiko>();
            }

            yield return ModelDb.Card<Slander>();
            yield return ModelDb.Card<Unendurable>();
        }
    }

    public override int StartingGold => 99;

    public override int StartingHp => 65;

    public override IReadOnlyList<RelicModel> StartingRelics => new[]
    {
        ModelDb.Relic<DollMask>()
    };

    public override bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> options, CardCreationOptions creationOptions)
    {
        bool modified = base.TryModifyCardRewardOptionsLate(player, options, creationOptions);
        CardCreationOptions eligibleOptions = CreateEligibleCardCreationOptions(creationOptions);
        bool replaced = ReplaceIneligibleCardResults(player, options, eligibleOptions, preserveOriginalRarity: false);
        bool toppedUp = TopUpRewardOptions(player, options, eligibleOptions, 3);
        return replaced || toppedUp || modified;
    }

    public override CardCreationOptions ModifyCardRewardCreationOptionsLate(Player player, CardCreationOptions options)
    {
        CardCreationOptions modified = base.ModifyCardRewardCreationOptionsLate(player, options);
        return CreateEligibleCardCreationOptions(modified);
    }

    public override void ModifyMerchantCardCreationResults(Player player, List<CardCreationResult> options)
    {
        base.ModifyMerchantCardCreationResults(player, options);
        ReplaceIneligibleCardResults(
            player,
            options,
            new CardCreationOptions(GetRewardEligiblePoolCards(), CardCreationSource.Shop, CardRarityOddsType.Shop),
            preserveOriginalRarity: true);
    }

    public override IEnumerable<CardModel> ModifyMerchantCardPool(Player player, IEnumerable<CardModel> options)
    {
        return base.ModifyMerchantCardPool(player, options).Where(IsRewardEligibleCard);
    }

    protected override CharacterModel? UnlocksAfterRunAs => null;

    public override List<string> GetArchitectAttackVfx()
    {
        return new List<string>
        {
            "vfx/vfx_dagger_spray",
            "vfx/vfx_flying_slash",
            "vfx/vfx_dramatic_stab",
            "vfx/vfx_dagger_throw"
        };
    }

    private static CardCreationOptions CreateEligibleCardCreationOptions(CardCreationOptions options)
    {
        Func<CardModel, bool> filter = card => (options.CardPoolFilter?.Invoke(card) ?? true) && IsRewardEligibleCard(card);
        if (options.CustomCardPool != null)
        {
            return options.WithCustomPool(options.CustomCardPool.Where(filter), options.RarityOdds);
        }

        return options.WithCardPools(options.CardPools, filter);
    }

    private static bool ReplaceIneligibleCardResults(
        Player player,
        IList<CardCreationResult> results,
        CardCreationOptions creationOptions,
        bool preserveOriginalRarity)
    {
        bool modified = false;
        for (int index = 0; index < results.Count; index++)
        {
            CardCreationResult result = results[index];
            if (IsRewardEligibleCard(result.Card))
            {
                continue;
            }

            CardRarity? requiredRarity = preserveOriginalRarity ? result.Card.Rarity : null;
            CardModel? replacement = CreateReplacementCard(player, creationOptions, requiredRarity);
            if (replacement == null)
            {
                ModSupport.LogWarn($"Could not replace ineligible card result {result.Card.Id}; leaving original result in place.");
                continue;
            }

            results[index] = new CardCreationResult(replacement);
            modified = true;
        }

        return modified;
    }

    private static bool TopUpRewardOptions(
        Player player,
        IList<CardCreationResult> results,
        CardCreationOptions creationOptions,
        int minimumCount)
    {
        bool modified = false;
        while (results.Count < minimumCount)
        {
            IReadOnlySet<string> excludedEntries = results
                .Select(result => result.Card.Id.Entry)
                .ToHashSet(StringComparer.Ordinal);

            CardModel? replacement = CreateReplacementCard(player, creationOptions, null, excludedEntries);
            if (replacement == null)
            {
                replacement = CreateReplacementCard(player, creationOptions, null);
            }

            if (replacement == null)
            {
                ModSupport.LogWarn($"Could not top up reward options to {minimumCount}; current count={results.Count}.");
                break;
            }

            results.Add(new CardCreationResult(replacement));
            modified = true;
        }

        return modified;
    }

    private static CardModel? CreateReplacementCard(
        Player player,
        CardCreationOptions creationOptions,
        CardRarity? requiredRarity,
        IReadOnlySet<string>? excludedEntries = null)
    {
        IReadOnlyList<CardModel> candidates = GetCandidatesFromCreationOptions(creationOptions, requiredRarity, excludedEntries);
        if (candidates.Count == 0 && requiredRarity != null)
        {
            candidates = GetCandidatesFromCreationOptions(creationOptions, null, excludedEntries);
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        CardCreationOptions replacementOptions = new(candidates, creationOptions.Source, creationOptions.RarityOdds);
        replacementOptions = replacementOptions.WithFlags(creationOptions.Flags);
        if (creationOptions.RngOverride != null)
        {
            replacementOptions = replacementOptions.WithRngOverride(creationOptions.RngOverride);
        }

        if (creationOptions.Source == CardCreationSource.Shop)
        {
            CardRarity merchantRarity = requiredRarity ?? candidates[0].Rarity;
            return CardFactory.CreateForMerchant(player, candidates, merchantRarity).Card;
        }

        return CardFactory.CreateForReward(player, 1, replacementOptions).FirstOrDefault()?.Card;
    }

    private static IReadOnlyList<CardModel> GetCandidatesFromCreationOptions(
        CardCreationOptions creationOptions,
        CardRarity? requiredRarity,
        IReadOnlySet<string>? excludedEntries = null)
    {
        IEnumerable<CardModel> candidates = creationOptions.CustomCardPool
            ?? creationOptions.CardPools.SelectMany(pool => pool.AllCards);

        candidates = candidates.Where(card => (creationOptions.CardPoolFilter?.Invoke(card) ?? true) && IsRewardEligibleCard(card));
        if (excludedEntries != null && excludedEntries.Count > 0)
        {
            candidates = candidates.Where(card => !excludedEntries.Contains(card.Id.Entry));
        }

        if (requiredRarity != null)
        {
            candidates = candidates.Where(card => card.Rarity == requiredRarity.Value);
        }

        return candidates.Distinct().ToList();
    }

    private static IReadOnlyList<CardModel> GetRewardEligiblePoolCards()
    {
        return ModelDb.CardPool<TogawasakikoCardPool>()
            .AllCards
            .Where(IsRewardEligibleCard)
            .ToList();
    }

    internal static bool IsRewardEligibleCard(CardModel? card)
    {
        return card != null
            && card.ShouldShowInCardLibrary
            && card.Rarity != CardRarity.Basic
            && !ModSupport.IsStarterDeckCard(card)
            && ModSupport.HasCardLocalization(card);
    }
}

internal sealed class TogawasakikoCardPool : CardPoolModel
{
    public override string Title => "togawasakiko";

    public override string CardFrameMaterialPath => "card_frame_red";

    public override Color DeckEntryCardColor => new(0.62f, 0.15f, 0.18f, 1f);

    public override string EnergyColorName => "togawasakiko";

    public override bool IsColorless => false;

    protected override CardModel[] GenerateAllCards()
    {
        return new CardModel[]
        {
            ModelDb.Card<StrikeTogawasakiko>(),
            ModelDb.Card<DefendTogawasakiko>(),
            ModelDb.Card<Slander>(),
            ModelDb.Card<Unendurable>(),
            ModelDb.Card<IHaveAscended>(),
            ModelDb.Card<Thrilled>(),
            ModelDb.Card<Completeness>(),
            ModelDb.Card<PerkUp>(),
            ModelDb.Card<Speak>(),
            ModelDb.Card<RestorationOfPower>(),
            ModelDb.Card<SheIsRadiant>(),
            ModelDb.Card<PutOnYourMask>(),
            ModelDb.Card<SeverThePast>(),
            ModelDb.Card<SoManyMaggots>(),
            ModelDb.Card<AnswerMe>(),
            ModelDb.Card<Notebook>(),
            ModelDb.Card<LeaveItToMe>(),
            ModelDb.Card<Fragility>(),
            ModelDb.Card<Compose>(),
            ModelDb.Card<DawnOfDespair>(),
            ModelDb.Card<FinalCurtain>(),
            ModelDb.Card<BladeThroughTheHeart>(),
            ModelDb.Card<AveMujica>(),
            ModelDb.Card<AWonderfulWorldYetNowhereToBeFound>(),
            ModelDb.Card<Angles>(),
            ModelDb.Card<Ether>(),
            ModelDb.Card<GeorgetteMeGeorgetteYou>(),
            ModelDb.Card<SymbolI>(),
            ModelDb.Card<SymbolIi>(),
            ModelDb.Card<SymbolIii>(),
            ModelDb.Card<SymbolIv>(),
            ModelDb.Card<CrucifixX>(),
            ModelDb.Card<Face>(),
            ModelDb.Card<SakiMovePlz>(),
            ModelDb.Card<MusicOfTheCelestialSphere>(),
            ModelDb.Card<KillKiss>(),
            ModelDb.Card<BlackBirthday>(),
            ModelDb.Card<TreasurePleasure>(),
            ModelDb.Card<BailMoney>(),
            ModelDb.Card<Innocence>(),
            ModelDb.Card<ChoirSChoir>(),
            ModelDb.Card<ImprisonedXii>(),
            ModelDb.Card<GodYouFool>(),
            ModelDb.Card<WeightliftingChampion>(),
            ModelDb.Card<Housewarming>(),
            ModelDb.Card<MasqueradeRhapsodyRequest>(),
            ModelDb.Card<STheWay>(),
            ModelDb.Card<TwoMoonsDeepIntoTheForest>(),
            ModelDb.Card<Sophie>()
        };
    }
}

internal sealed class TogawasakikoPotionPool : PotionPoolModel
{
    public override string EnergyColorName => "togawasakiko";

    protected override IEnumerable<PotionModel> GenerateAllPotions()
    {
        return Array.Empty<PotionModel>();
    }
}

internal sealed class TogawasakikoRelicGrantedCardPool : CardPoolModel
{
    public override string Title => "togawasakiko_relic_granted";

    public override string CardFrameMaterialPath => "card_frame_red";

    public override Color DeckEntryCardColor => new(0.62f, 0.15f, 0.18f, 1f);

    public override string EnergyColorName => "togawasakiko";

    public override bool IsColorless => false;

    protected override CardModel[] GenerateAllCards()
    {
        return Array.Empty<CardModel>();
    }
}

internal sealed class TogawasakikoEventGrantedCardPool : CardPoolModel
{
    public override string Title => "togawasakiko_event_granted";

    public override string CardFrameMaterialPath => "card_frame_red";

    public override Color DeckEntryCardColor => new(0.53f, 0.20f, 0.27f, 1f);

    public override string EnergyColorName => "togawasakiko";

    public override bool IsColorless => false;

    protected override CardModel[] GenerateAllCards()
    {
        return Array.Empty<CardModel>();
    }
}

internal sealed class TogawasakikoRelicPool : RelicPoolModel
{
    public override string EnergyColorName => "togawasakiko";

    protected override IEnumerable<RelicModel> GenerateAllRelics()
    {
        return new RelicModel[]
        {
            ModelDb.Relic<DollMask>()
        };
    }
}

internal sealed class TogawasakikoAncientRelicPool : RelicPoolModel
{
    public override string EnergyColorName => "togawasakiko";

    protected override IEnumerable<RelicModel> GenerateAllRelics()
    {
        return Array.Empty<RelicModel>();
    }
}

internal sealed class TogawasakikoSpecialRelicPool : RelicPoolModel
{
    public override string EnergyColorName => "togawasakiko";

    protected override IEnumerable<RelicModel> GenerateAllRelics()
    {
        return new RelicModel[]
        {
            ModelDb.Relic<UpgradedDollMask>()
        };
    }
}
