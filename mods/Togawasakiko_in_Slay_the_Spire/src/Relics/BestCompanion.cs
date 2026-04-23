using System.Threading.Tasks;
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

        if (ModSupport.AddSpecificCardToDeck<BarkingBarkingBarking>(Owner) != null)
        {
            Flash();
        }

        await Task.CompletedTask;
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

        if (ModSupport.AddSpecificCardToDeck<PullmanCrash>(Owner) != null)
        {
            Flash();
        }

        await Task.CompletedTask;
    }
}
