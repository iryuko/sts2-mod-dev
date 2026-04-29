using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Togawasakiko_in_Slay_the_Spire;

internal sealed class PianoOfMom : RelicModel
{
    protected override string IconBaseName => "piano_of_mom";

    public override string PackedIconPath => "res://images/atlases/relic_atlas.sprites/piano_of_mom.tres";

    protected override string PackedIconOutlinePath => "res://images/atlases/relic_outline_atlas.sprites/piano_of_mom.tres";

    protected override string BigIconPath => "res://images/relics/piano_of_mom.png";

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
    {
        if (Owner != player || player.Creature?.CombatState == null || player.Creature.CombatState != combatState)
        {
            return;
        }

        if (combatState.RoundNumber > 1)
        {
            return;
        }

        CardModel? card = await ModSupport.GiveRandomUpgradedSongCardToPlayer(player, this);
        if (card != null)
        {
            Flash();
        }
    }
}
