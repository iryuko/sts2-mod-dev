using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Togawasakiko_in_Slay_the_Spire;

internal sealed class UpgradedDollMask : RelicModel
{
    protected override string IconBaseName => "doll_mask";

    public override string PackedIconPath => "res://images/atlases/relic_atlas.sprites/doll_mask.tres";

    protected override string PackedIconOutlinePath => "res://images/atlases/relic_outline_atlas.sprites/doll_mask.tres";

    protected override string BigIconPath => "res://images/relics/doll_mask.png";

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner != player || player.Creature == null)
        {
            return;
        }

        foreach (var enemy in ModSupport.GetEnemyCreatures(player.Creature))
        {
            await ModSupport.ApplyPressure(enemy, 3m, player.Creature, null);
        }

        Flash();
    }
}
