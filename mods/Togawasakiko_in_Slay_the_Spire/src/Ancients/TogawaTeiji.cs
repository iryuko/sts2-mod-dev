using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Togawasakiko_in_Slay_the_Spire;

internal sealed class TogawaTeiji : AncientEventModel
{
    private const string LocPrefix = "TOGAWA_TEIJI";
    private static readonly string[] EmptyDialogueAudio = { "", "", "", "" };

    public override LocString InitialDescription => Loc($"{LocPrefix}.pages.INITIAL.description");

    public override IEnumerable<EventOption> AllPossibleOptions => CreateAllPossibleOptions();

    protected override AncientDialogueSet DefineDialogues()
    {
        string togawasakikoKey = ModelDb.Character<Togawasakiko>().Id.Entry;
        AncientDialogue repeatedCharacterDialogue = new(EmptyDialogueAudio)
        {
            VisitIndex = 0,
            IsRepeating = true
        };
        AncientDialogueSet dialogueSet = new()
        {
            FirstVisitEverDialogue = new AncientDialogue(EmptyDialogueAudio),
            CharacterDialogues = new Dictionary<string, IReadOnlyList<AncientDialogue>>
            {
                [togawasakikoKey] = new AncientDialogue[]
                {
                    repeatedCharacterDialogue
                }
            },
            AgnosticDialogues = Array.Empty<AncientDialogue>()
        };
        dialogueSet.PopulateLocKeys(LocPrefix);
        return dialogueSet;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return CreateAllPossibleOptions();
    }

    private IReadOnlyList<EventOption> CreateAllPossibleOptions()
    {
        return new EventOption[]
        {
            CreateRelicOptionWithCardPreview<BestCompanion, BarkingBarkingBarking>(),
            CreateRelicOptionWithCardPreview<BlackLimousine, PullmanCrash>(),
            CreateContinuePerformingOption()
        };
    }

    private EventOption CreateRelicOptionWithCardPreview<TRelic, TCard>()
        where TRelic : RelicModel
        where TCard : CardModel
    {
        EventOption option = RelicOption<TRelic>();
        option.HoverTips = option.HoverTips.Concat(new[]
        {
            ModSupport.CreateCardHoverTip<TCard>()
        });
        return option;
    }

    private EventOption CreateContinuePerformingOption()
    {
        return new EventOption(
            this,
            ContinuePerforming,
            Loc($"{LocPrefix}.pages.INITIAL.options.CONTINUE_PERFORMING.title"),
            Loc($"{LocPrefix}.pages.INITIAL.options.CONTINUE_PERFORMING.description"),
            "CONTINUE_PERFORMING",
            Array.Empty<IHoverTip>());
    }

    private async Task ContinuePerforming()
    {
        if (Owner != null)
        {
            await PlayerCmd.GainGold(1000m, Owner);
        }

        Done();
    }

    private static LocString Loc(string key)
    {
        return new("ancients", key);
    }
}
