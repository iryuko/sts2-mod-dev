using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

internal static class CastAnimationRegression
{
    private static readonly List<(Creature Creature, string Trigger, float Delay, int Block)> Requests = [];

    private static void Equal<T>(T expected, T actual)
    {
        if (!Equals(expected, actual)) throw new Exception($"expected {expected}, got {actual}");
    }

    // Observe the actual native animation-command boundary. The command still runs;
    // TestMode has no creature scene, so this does not assert rendered animation.
    private static void Observe(Creature creature, string triggerName, float waitTime) =>
        Requests.Add((creature, triggerName, waitTime, creature.Block));

    private static void ExpectCast(Creature owner, int count = 1)
    {
        var own = Requests.Where(r => r.Creature == owner).ToArray();
        Equal(count, own.Length);
        foreach (var request in own)
        {
            Equal("Cast", request.Trigger);
            Equal(0.25f, request.Delay);
        }
    }

    internal static async Task Run(Func<string, Func<Task>, Task> check)
    {
        var harmony = new Harmony("sakiko.cast-animation-regression");
        harmony.Patch(AccessTools.Method(typeof(CreatureCmd), nameof(CreatureCmd.TriggerAnim)),
            prefix: new HarmonyMethod(typeof(CastAnimationRegression), nameof(Observe)));
        try
        {
            foreach (string name in new[] { "DefendTogawasakiko", "Innocence", "PersonaDissociation", "SocialWithdrawal", "OverworkAnxiety" })
            {
                await check($"Cast is requested by native card playback: {name}", async () =>
                {
                    var f = new CombatFixture();
                    Requests.Clear();
                    CardModel card = f.Card(name);
                    await CardCmd.AutoPlay(f.Choice, card, card.TargetType == TargetType.AnyEnemy ? f.Enemy : null);
                    ExpectCast(f.Players[0].Creature);
                    if (name == "DefendTogawasakiko")
                    {
                        Equal(0, Requests.Single().Block);
                        Equal(5, f.Players[0].Creature.Block);
                    }
                    if (name == "Innocence")
                        Equal(2, f.Players[0].Creature.Powers.Single(p => p.Id.Entry == "INNOCENCE_POWER").Amount);
                });
            }

            await check("Attack pressure token casts once without an Attack interrupt and retains damage/stun", async () =>
            {
                var f = new CombatFixture();
                Requests.Clear();
                int hp = f.Enemy.CurrentHp;
                await CardCmd.AutoPlay(f.Choice, f.Card("AllYouThinkAboutIsYourself"), f.Enemy);
                ExpectCast(f.Players[0].Creature);
                Equal(hp - 9, f.Enemy.CurrentHp);
                Equal("STUNNED", f.Enemy.Monster!.NextMove.Id);
            });

            await check("Ordinary attacks retain their Attack animation", async () =>
            {
                var f = new CombatFixture();
                Requests.Clear();
                await CardCmd.AutoPlay(f.Choice, f.Card("StrikeTogawasakiko"), f.Enemy);
                var own = Requests.Where(r => r.Creature == f.Players[0].Creature).ToArray();
                Equal(1, own.Length);
                Equal("Attack", own[0].Trigger);
                Equal(0.31f, own[0].Delay);
            });

            await check("Only the played instance animates, despite copies and another Sakiko", async () =>
            {
                var f = new CombatFixture("Togawasakiko", "Togawasakiko");
                await CardPileCmd.Add(f.Card("DefendTogawasakiko"), PileType.Hand);
                await CardPileCmd.Add(f.Card("DefendTogawasakiko", 1), PileType.Hand);
                CardModel played = f.Card("DefendTogawasakiko", 1);
                await CardPileCmd.Add(played, PileType.Play);
                Requests.Clear();
                await Hook.BeforeCardPlayed(f.State, CombatFixture.Play(played));
                Equal(1, Requests.Count);
                ExpectCast(f.Players[1].Creature);
            });

            await check("Native Burst requests one Cast per actual repeat and keeps token exhaustion", async () =>
            {
                var f = new CombatFixture();
                ModelDb.Power<BurstPower>().ToMutable().ApplyInternal(f.Players[0].Creature, 1, true);
                CardModel card = f.Card("OverworkAnxiety");
                Requests.Clear();
                await CardCmd.AutoPlay(f.Choice, card, null);
                ExpectCast(f.Players[0].Creature, 2);
                Equal(PileType.Exhaust, card.Pile!.Type);
            });

            await check("Vanilla Cast cards do not receive a duplicate from Sakiko cards", async () =>
            {
                var f = new CombatFixture();
                await CardPileCmd.Add(f.Card("DefendTogawasakiko"), PileType.Hand);
                CardModel card = f.State.CreateCard(ModelDb.Card<DarkShackles>(), f.Players[0]);
                Requests.Clear();
                await CardCmd.AutoPlay(f.Choice, card, f.Enemy);
                ExpectCast(f.Players[0].Creature);
            });
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
            Requests.Clear();
        }
    }
}
