using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Togawasakiko_in_Slay_the_Spire;

[ModInitializer("Initialize")]
public static class TogawasakikoMod
{
    private const string HarmonyId = "Togawasakiko_in_Slay_the_Spire";

    private static RunState? _activeRun;
    private static bool _shadowDeckSanitizedForActiveRun;
    private static ModSupport.CharacterSelectInjector? _characterSelectInjector;

    public static void Initialize()
    {
        ModSupport.LogInfo("Initialize start.");

        ModSupport.EnsureLocalizationOverrides();
        new Harmony(HarmonyId).PatchAll();
        RegisterSavedPropertyTypes();

        ModHelper.AddModelToPool<TokenCardPool, PersonaDissociation>();
        ModHelper.AddModelToPool<TokenCardPool, SocialWithdrawal>();
        ModHelper.AddModelToPool<TokenCardPool, AllYouThinkAboutIsYourself>();
        ModHelper.AddModelToPool<TokenCardPool, OverworkAnxiety>();
        ModHelper.AddModelToPool<TogawasakikoEventGrantedCardPool, ShadowOfThePastI>();
        ModHelper.AddModelToPool<TogawasakikoEventGrantedCardPool, ShadowOfThePastII>();
        ModHelper.AddModelToPool<TogawasakikoEventGrantedCardPool, ShadowOfThePastIII>();
        ModHelper.AddModelToPool<TogawasakikoRelicGrantedCardPool, BarkingBarkingBarking>();
        ModHelper.AddModelToPool<TogawasakikoRelicGrantedCardPool, PullmanCrash>();
        ModHelper.AddModelToPool<TogawasakikoAncientRelicPool, BestCompanion>();
        ModHelper.AddModelToPool<TogawasakikoAncientRelicPool, BlackLimousine>();
        ModHelper.AddModelToPool<TogawasakikoSpecialRelicPool, UpgradedDollMask>();
        ModHelper.AddModelToPool<TogawasakikoSpecialRelicPool, PianoOfMom>();

        RunManager.Instance.RunStarted += OnRunStarted;
        RunManager.Instance.RoomEntered += OnRoomEntered;
        ModSupport.LogInfo("Registered token/special cards, ancient relic stubs, character-list patch, localization overrides, and runtime hooks.");
    }

    private static void RegisterSavedPropertyTypes()
    {
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(SymbolIii));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(ShadowOfThePastI));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(ShadowOfThePastII));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(ShadowOfThePastIII));

        ModSupport.LogInfo("Registered mod card saved-property types.");
    }

    private static void InstallCharacterSelectInjector()
    {
        if (_characterSelectInjector != null)
        {
            return;
        }

        if (Engine.GetMainLoop() is not SceneTree tree || tree.Root == null)
        {
            ModSupport.LogWarn("SceneTree not ready. Character select injector not installed.");
            return;
        }

        _characterSelectInjector = new ModSupport.CharacterSelectInjector();
        _ = InstallCharacterSelectInjectorAsync(tree, _characterSelectInjector);
        ModSupport.LogInfo("Scheduled async character select injector install.");
    }

    private static async Task InstallCharacterSelectInjectorAsync(SceneTree tree, Node injector)
    {
        try
        {
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);

            if (tree.Root == null)
            {
                ModSupport.LogWarn("SceneTree root disappeared before injector install.");
                return;
            }

            tree.Root.AddChild(injector);
            ModSupport.LogInfo("Installed character select injector on root.");
        }
        catch (Exception ex)
        {
            ModSupport.LogError("Failed to install character select injector: " + ex);
        }
    }

    private static void OnRunStarted(RunState runState)
    {
        _activeRun = runState;
        _shadowDeckSanitizedForActiveRun = false;
        SanitizeShadowCards(runState);

        foreach (Player player in runState.Players)
        {
            ModSupport.ClearPersistedTwoMoonsCostModifiers(player);

            if (player.Character == null)
            {
                ModSupport.LogWarn($"RunStarted player={player.NetId} has no character.");
                continue;
            }

            string starterRelics = string.Join(
                ", ",
                player.Character.StartingRelics.Select(relic => $"{relic.Id.Entry}:{relic.GetType().Name}"));

            ModSupport.LogInfo(
                $"RunStarted player={player.NetId} character={player.Character.Id.Entry} type={player.Character.GetType().FullName} starterRelics=[{starterRelics}]");
        }
    }

    private static void OnRoomEntered()
    {
        RunState? runState = _activeRun;
        if (runState != null && !_shadowDeckSanitizedForActiveRun)
        {
            SanitizeShadowCards(runState);
        }

        JukeboxRunInjector.HandleRoomEntered(runState?.CurrentRoom);
    }

    private static void SanitizeShadowCards(RunState runState)
    {
        int replaced = 0;
        foreach (Player player in runState.Players)
        {
            CardPile? deck = player.Deck;
            if (deck == null)
            {
                continue;
            }

            for (int index = 0; index < deck.Cards.Count; index++)
            {
                if (deck.Cards[index] is not ShadowOfThePastCard shadow)
                {
                    continue;
                }

                ShadowOfThePastCard freshShadow = CreateFreshShadowCard(runState, player, shadow);
                if (ReferenceEquals(freshShadow, shadow))
                {
                    continue;
                }

                deck.RemoveInternal(shadow, silent: true);
                runState.RemoveCard(shadow);
                deck.AddInternal(freshShadow, index, silent: true);
                replaced++;
            }
        }

        _shadowDeckSanitizedForActiveRun = true;
        if (replaced > 0)
        {
            ModSupport.LogInfo($"Sanitized {replaced} Shadow card instance(s) across all active decks.");
        }
    }

    private static ShadowOfThePastCard CreateFreshShadowCard(RunState runState, Player player, ShadowOfThePastCard shadow)
    {
        ShadowOfThePastCard freshShadow = shadow switch
        {
            ShadowOfThePastI => (ShadowOfThePastCard)runState.CreateCard(ModelDb.Card<ShadowOfThePastI>(), player),
            ShadowOfThePastII => (ShadowOfThePastCard)runState.CreateCard(ModelDb.Card<ShadowOfThePastII>(), player),
            ShadowOfThePastIII => (ShadowOfThePastCard)runState.CreateCard(ModelDb.Card<ShadowOfThePastIII>(), player),
            _ => shadow
        };

        if (!ReferenceEquals(freshShadow, shadow))
        {
            freshShadow.CombatsSeen = shadow.CombatsSeen;
            freshShadow.FloorAddedToDeck = shadow.FloorAddedToDeck;
        }

        return freshShadow;
    }

    public static Player? GetLocalPlayer(RunState? runState)
    {
        if (runState == null)
        {
            return null;
        }

        Player? localPlayer = LocalContext.GetMe(runState);
        if (localPlayer != null)
        {
            return localPlayer;
        }

        if (runState.Players.Count == 1)
        {
            return runState.Players[0];
        }

        ModSupport.LogWarn("GetLocalPlayer could not resolve LocalContext in multiplayer run; refusing ambiguous FirstOrDefault fallback.");
        return null;
    }
}
