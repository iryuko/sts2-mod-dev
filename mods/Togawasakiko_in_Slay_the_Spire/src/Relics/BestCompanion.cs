using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Togawasakiko_in_Slay_the_Spire;

internal sealed class BestCompanion : RelicModel
{
    protected override string IconBaseName => "best_companion";

    public override string PackedIconPath => "res://images/atlases/relic_atlas.sprites/best_companion.tres";

    protected override string PackedIconOutlinePath => "res://images/atlases/relic_outline_atlas.sprites/best_companion.tres";

    protected override string BigIconPath => "res://images/relics/best_companion.png";

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override async Task AfterObtained()
    {
        if (Owner == null)
        {
            return;
        }

        CardModel card = Owner.RunState.CreateCard(
            ModelDb.Card<BarkingBarkingBarking>(),
            Owner);
        CardCmd.PreviewCardPileAdd(
            await CardPileCmd.Add(card, PileType.Deck),
            2f);
        Flash();
    }
}

internal sealed class BlackLimousine : RelicModel
{
    protected override string IconBaseName => "black_limousine";

    public override string PackedIconPath => "res://images/atlases/relic_atlas.sprites/black_limousine.tres";

    protected override string PackedIconOutlinePath => "res://images/atlases/relic_outline_atlas.sprites/black_limousine.tres";

    protected override string BigIconPath => "res://images/relics/black_limousine.png";

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override async Task AfterObtained()
    {
        if (Owner == null)
        {
            return;
        }

        CardModel card = Owner.RunState.CreateCard(
            ModelDb.Card<PullmanCrash>(),
            Owner);
        CardCmd.PreviewCardPileAdd(
            await CardPileCmd.Add(card, PileType.Deck),
            2f);
        Flash();
    }
}
