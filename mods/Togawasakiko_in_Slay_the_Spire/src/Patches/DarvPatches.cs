using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch(typeof(Darv), "GenerateInitialOptions")]
internal static class DarvGenerateInitialOptionsPatch
{
    private static readonly MethodInfo RelicOptionMethod =
        AccessTools.Method(
            typeof(AncientEventModel),
            "RelicOption",
            new[] { typeof(RelicModel), typeof(string), typeof(string) });

    [HarmonyPrefix]
    private static bool GenerateTogawasakikoSafeOptions(Darv __instance, ref IReadOnlyList<EventOption> __result)
    {
        if (__instance.Owner is not { Character: Togawasakiko } owner)
        {
            return true;
        }

        List<EventOption> source = CreateRelicOptions(__instance, owner).UnstableShuffle(__instance.Rng);
        bool dustyTomeRoll = __instance.Rng.NextBool();
        ModSupport.LogInfo(
            $"Darv options for Togawasakiko: act={owner.RunState.CurrentActIndex + 1} dustyTomeRoll={dustyTomeRoll} relicOptions={source.Count}.");

        if (dustyTomeRoll && TryCreateDustyTomeOption(__instance, owner, out EventOption? dustyTomeOption))
        {
            List<EventOption> withTome = source.Take(2).ToList();
            if (dustyTomeOption != null)
            {
                withTome.Add(dustyTomeOption);
            }

            ModSupport.LogInfo($"Darv options for Togawasakiko include DustyTome; optionCount={withTome.Count}.");
            __result = withTome;
        }
        else
        {
            __result = source.Take(3).ToList();
            ModSupport.LogInfo($"Darv options for Togawasakiko use relic-only fallback; optionCount={__result.Count}.");
        }

        return false;
    }

    private static List<EventOption> CreateRelicOptions(Darv darv, Player owner)
    {
        List<EventOption> options = new()
        {
            CreateRelicOption<Astrolabe>(darv),
            CreateRelicOption<BlackStar>(darv),
            CreateRelicOption<CallingBell>(darv),
            CreateRelicOption<EmptyCage>(darv),
            CreateRelicOption<RunicPyramid>(darv),
            CreateRelicOption<SneckoEye>(darv)
        };

        if (!owner.RunState.Modifiers.Any(modifier => modifier.ClearsPlayerDeck))
        {
            options.Add(CreateRelicOption<PandorasBox>(darv));
        }

        if (owner.RunState.CurrentActIndex == 1)
        {
            options.Add(CreateRelicOption(darv, darv.Rng.NextItem(new RelicModel[]
            {
                ModelDb.Relic<Ectoplasm>(),
                ModelDb.Relic<Sozu>()
            })!.ToMutable()));
        }
        else if (owner.RunState.CurrentActIndex == 2)
        {
            options.Add(CreateRelicOption(darv, darv.Rng.NextItem(new RelicModel[]
            {
                ModelDb.Relic<PhilosophersStone>(),
                ModelDb.Relic<VelvetChoker>()
            })!.ToMutable()));
        }

        return options;
    }

    private static bool TryCreateDustyTomeOption(Darv darv, Player owner, out EventOption? option)
    {
        option = null;
        List<CardModel> ancientCandidates = owner.Character.CardPool
            .GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint)
            .Where(card => card.Rarity == CardRarity.Ancient && !ArchaicTooth.TranscendenceCards.Contains(card))
            .ToList();
        string candidateIds = string.Join(",", ancientCandidates.Select(card => card.Id.Entry));
        ModSupport.LogInfo(
            $"Darv DustyTome candidates for Togawasakiko: count={ancientCandidates.Count} ids=[{candidateIds}].");

        if (ancientCandidates.Count == 0)
        {
            ModSupport.LogInfo("Skipping Darv Dusty Tome option for Togawasakiko because her card pool has no Ancient-rarity cards.");
            return false;
        }

        DustyTome dustyTome = (DustyTome)ModelDb.Relic<DustyTome>().ToMutable();
        dustyTome.SetupForPlayer(owner);
        option = CreateRelicOption(darv, dustyTome);
        return true;
    }

    private static EventOption CreateRelicOption<TRelic>(Darv darv)
        where TRelic : RelicModel
    {
        return CreateRelicOption(darv, ModelDb.Relic<TRelic>().ToMutable());
    }

    private static EventOption CreateRelicOption(Darv darv, RelicModel relic)
    {
        return (EventOption)RelicOptionMethod.Invoke(darv, new object?[] { relic, "INITIAL", null })!;
    }
}
