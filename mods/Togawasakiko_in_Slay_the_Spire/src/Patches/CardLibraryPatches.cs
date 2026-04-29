using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllCardPools), MethodType.Getter)]
internal static class TogawasakikoAllCardPoolsPatch
{
    [HarmonyPostfix]
    private static IEnumerable<CardPoolModel> AppendTogawaCardPool(IEnumerable<CardPoolModel> __result)
    {
        CardPoolModel togawaPool = ModelDb.CardPool<TogawasakikoCardPool>();
        if (__result.Any(pool => pool.Id == togawaPool.Id))
        {
            return __result;
        }

        return __result.Concat(new[] { togawaPool });
    }
}

[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllCards), MethodType.Getter)]
internal static class TogawasakikoAllCardsPatch
{
    [HarmonyPostfix]
    private static IEnumerable<CardModel> AppendTogawaCards(IEnumerable<CardModel> __result)
    {
        List<CardModel> cards = __result.ToList();
        HashSet<ModelId> existingIds = cards.Select(card => card.Id).ToHashSet();

        foreach (CardModel card in ModelDb.CardPool<TogawasakikoCardPool>().AllCards)
        {
            if (existingIds.Add(card.Id))
            {
                cards.Add(card);
            }
        }

        return cards;
    }
}

[HarmonyPatch(typeof(NCardLibrary), nameof(NCardLibrary._Ready))]
internal static class CardLibraryTogawasakikoPoolPatch
{
    private const string TogawasakikoPoolNodeName = "TogawasakikoPool";
    private const string PoolToggleScenePath = "res://scenes/screens/card_library/library_pool_toggle.tscn";

    private static readonly AccessTools.FieldRef<NCardLibrary, Dictionary<NCardPoolFilter, Func<CardModel, bool>>> PoolFiltersRef =
        AccessTools.FieldRefAccess<NCardLibrary, Dictionary<NCardPoolFilter, Func<CardModel, bool>>>("_poolFilters");

    private static readonly AccessTools.FieldRef<NCardLibrary, Dictionary<CharacterModel, NCardPoolFilter>> CardPoolFiltersRef =
        AccessTools.FieldRefAccess<NCardLibrary, Dictionary<CharacterModel, NCardPoolFilter>>("_cardPoolFilters");

    private static readonly AccessTools.FieldRef<NCardLibrary, Control?> LastHoveredControlRef =
        AccessTools.FieldRefAccess<NCardLibrary, Control?>("_lastHoveredControl");

    [HarmonyPostfix]
    private static void AddTogawasakikoPoolFilter(NCardLibrary __instance)
    {
        try
        {
            AddTogawasakikoPoolFilterUnsafe(__instance);
        }
        catch (Exception ex)
        {
            ModSupport.LogWarn("Failed to inject Togawasakiko card-library pool filter: " + ex);
        }
    }

    private static void AddTogawasakikoPoolFilterUnsafe(NCardLibrary cardLibrary)
    {
        Dictionary<NCardPoolFilter, Func<CardModel, bool>> poolFilters = PoolFiltersRef(cardLibrary);
        Dictionary<CharacterModel, NCardPoolFilter> cardPoolFilters = CardPoolFiltersRef(cardLibrary);
        CharacterModel togawasakiko = ModelDb.Character<Togawasakiko>();

        if (cardPoolFilters.ContainsKey(togawasakiko))
        {
            return;
        }

        GridContainer poolFilterContainer = cardLibrary.GetNode<GridContainer>("Sidebar/MarginContainer/TopVBox/PoolFilters");
        if (poolFilterContainer.GetNodeOrNull(TogawasakikoPoolNodeName) is NCardPoolFilter existingFilter)
        {
            RegisterFilter(cardLibrary, existingFilter, poolFilters, cardPoolFilters, togawasakiko);
            return;
        }

        PackedScene? toggleScene = ResourceLoader.Load<PackedScene>(PoolToggleScenePath);
        if (toggleScene == null)
        {
            ModSupport.LogWarn("Card-library pool toggle scene missing: " + PoolToggleScenePath);
            return;
        }

        NCardPoolFilter togawaFilter = toggleScene.Instantiate<NCardPoolFilter>();
        togawaFilter.Name = TogawasakikoPoolNodeName;
        togawaFilter.Loc = new LocString("card_library", "POOL_TOGAWASAKIKO_TIP");
        poolFilterContainer.AddChild(togawaFilter);
        ApplyIconTexture(togawaFilter, ModelDb.Character<Togawasakiko>().IconTexture);
        RegisterFilter(cardLibrary, togawaFilter, poolFilters, cardPoolFilters, togawasakiko);
    }

    private static void RegisterFilter(
        NCardLibrary cardLibrary,
        NCardPoolFilter togawaFilter,
        Dictionary<NCardPoolFilter, Func<CardModel, bool>> poolFilters,
        Dictionary<CharacterModel, NCardPoolFilter> cardPoolFilters,
        CharacterModel togawasakiko)
    {
        poolFilters[togawaFilter] = card => card.Pool is TogawasakikoCardPool;
        cardPoolFilters[togawasakiko] = togawaFilter;
        togawaFilter.Connect(NCardPoolFilter.SignalName.Toggled, Callable.From<NCardPoolFilter>(filter =>
        {
            cardLibrary.Call(NCardLibrary.MethodName.UpdateCardPoolFilter, filter);
        }));
        togawaFilter.Connect(Control.SignalName.FocusEntered, Callable.From(() =>
        {
            LastHoveredControlRef(cardLibrary) = togawaFilter;
        }));
    }

    private static void ApplyIconTexture(NCardPoolFilter togawaFilter, Texture2D iconTexture)
    {
        if (togawaFilter.GetNodeOrNull<TextureRect>("Image") is TextureRect image)
        {
            image.Texture = iconTexture;
            if (image.GetNodeOrNull<TextureRect>("Shadow") is TextureRect shadow)
            {
                shadow.Texture = iconTexture;
            }
        }
    }
}
