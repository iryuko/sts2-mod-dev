using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers.Mocks;
using Togawasakiko_in_Slay_the_Spire;

internal static class EnemyTargetRegression
{
    internal static async Task Run(Func<string, Func<Task>, Task> check)
    {
        TestLocalization.Load("eng");

        await check("Enemy helper matches native HittableEnemies, excluding allied Osty and dead enemies", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Necrobinder");
            Creature osty = await PlayerCmd.AddPet<Osty>(f.Players[1]);
            Creature second = AddEnemy(f);
            Creature dead = AddEnemy(f);
            dead.SetCurrentHpInternal(0);

            Equal(CombatSide.Player, osty.Side);
            Equal(true, osty.IsMonster);
            Equal(true, osty.IsAlive);
            Equal(true, osty.IsHittable);
            Equal(true, f.State.HittableEnemies.SequenceEqual(new[] { f.Enemy, second }));
            Equal(true, GetEnemies(f.Players[0].Creature).SequenceEqual(f.State.HittableEnemies));
            // Enemy-owned effects also need the absolute enemy side, not the owner's opponents.
            Equal(true, GetEnemies(f.Enemy).SequenceEqual(f.State.HittableEnemies));
        });

        await check("Enemy helper respects native ShouldAllowHitting for a living revived enemy", async () =>
        {
            var f = new CombatFixture();
            var revive = ModelDb.Power<MockRevivePower>().ToMutable();
            revive.ApplyInternal(f.Enemy, 1, true);
            f.Enemy.SetCurrentHpInternal(0);
            await revive.AfterPreventingDeath(f.Enemy);

            Equal(true, f.Enemy.IsAlive);
            Equal(false, f.Enemy.IsHittable);
            Equal(0, f.State.HittableEnemies.Count);
            Equal(0, GetEnemies(f.Players[0].Creature).Length);
        });

        await check("Enemy helper returns no targets outside combat", () =>
        {
            var f = new CombatFixture();
            Creature owner = f.Players[0].Creature;
            owner.CombatState = null;
            Equal(0, GetEnemies(owner).Length);
            return Task.CompletedTask;
        });

        await check("Innocence applies Social Withdrawal to enemies but never allied Osty", async () =>
        {
            var f = new CombatFixture("Togawasakiko", "Necrobinder");
            Creature osty = await PlayerCmd.AddPet<Osty>(f.Players[1]);
            Creature second = AddEnemy(f);
            PowerModel power = CombatFixture.Canonical<PowerModel>("InnocencePower").ToMutable();
            power.ApplyInternal(f.Players[0].Creature, 2, true);

            await power.AfterPlayerTurnStartLate(f.Choice, f.Players[0]);

            Equal(2, PowerAmount(f.Enemy, "SOCIAL_WITHDRAWAL_POWER"));
            Equal(2, PowerAmount(second, "SOCIAL_WITHDRAWAL_POWER"));
            Equal(0, PowerAmount(osty, "SOCIAL_WITHDRAWAL_POWER"));
            Equal(0, PowerAmount(f.Players[0].Creature, "SOCIAL_WITHDRAWAL_POWER"));
            Equal(0, PowerAmount(f.Players[1].Creature, "SOCIAL_WITHDRAWAL_POWER"));
        });

        foreach (bool upgraded in new[] { false, true })
        {
            await check($"KillKiss damages pressured enemies but never allied Osty: upgraded={upgraded}", async () =>
            {
                var f = new CombatFixture("Togawasakiko", "Necrobinder");
                Creature osty = await PlayerCmd.AddPet<Osty>(f.Players[1]);
                Creature thresholdEnemy = AddEnemy(f);
                foreach (Creature creature in new[] { f.Enemy, osty, thresholdEnemy })
                {
                    creature.SetMaxHpInternal(100);
                    creature.SetCurrentHpInternal(100);
                }
                foreach (Creature creature in new[] { f.Enemy, osty })
                {
                    CombatFixture.Canonical<PowerModel>("PressurePower").ToMutable().ApplyInternal(creature, 60, true);
                    // Keep the max-HP variant nonlethal so this test isolates target selection.
                    if (upgraded) creature.GainBlockInternal(50);
                }
                CombatFixture.Canonical<PowerModel>("PressurePower").ToMutable().ApplyInternal(thresholdEnemy, 50, true);
                PowerModel power = CombatFixture.Canonical<PowerModel>(upgraded ? "KillKissPlusPower" : "KillKissPower").ToMutable();
                power.ApplyInternal(f.Players[0].Creature, 1, true);

                await power.AfterSideTurnStart(CombatSide.Enemy, f.State.Enemies, f.State);

                Equal(upgraded ? 50 : 75, f.Enemy.CurrentHp);
                Equal(100, thresholdEnemy.CurrentHp);
                Equal(100, osty.CurrentHp);
                Equal(upgraded ? 50 : 0, osty.Block);
            });
        }
    }

    private static Creature AddEnemy(CombatFixture f)
    {
        Creature enemy = f.State.CreateCreature(ModelDb.Monster<TestSubject>().ToMutable(), CombatSide.Enemy, null);
        f.State.AddCreature(enemy);
        return enemy;
    }

    private static Creature[] GetEnemies(Creature owner)
    {
        Type support = typeof(TogawasakikoMod).Assembly.GetType("Togawasakiko_in_Slay_the_Spire.ModSupport", true)!;
        return ((IEnumerable<Creature>)support.GetMethod("GetEnemyCreatures")!.Invoke(null, [owner])!).ToArray();
    }

    private static int PowerAmount(Creature creature, string entry)
    {
        return creature.Powers.FirstOrDefault(power => power.Id.Entry == entry)?.Amount ?? 0;
    }

    private static void Equal<T>(T expected, T actual)
    {
        if (!Equals(expected, actual)) throw new Exception($"expected {expected}, got {actual}");
    }
}
