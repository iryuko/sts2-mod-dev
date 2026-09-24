using System.Reflection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.TestSupport;
using Togawasakiko_in_Slay_the_Spire;

TestMode.TurnOnInternal();
NativeTestHost.Initialize();
// The headless fixture skips disk/Steam loading, not gameplay hooks.
typeof(ModManager).GetProperty(nameof(ModManager.State))!.SetValue(null, ModManagerState.Skipped);
ModelDb.Init();
foreach (Type type in typeof(TogawasakikoMod).Assembly.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(AbstractModel))))
    ModelDb.Inject(type);

void Equal<T>(T expected, T actual) { if (!Equals(expected, actual)) throw new Exception($"expected {expected}, got {actual}"); }

int passed = 0;
int failures = 0;
async Task Check(string name, Func<Task> action)
{
    try { await action(); passed++; Console.WriteLine("PASS " + name); }
    catch (Exception ex) { failures++; Console.WriteLine("FAIL " + name + ": " + ex); }
}

await Check("Two Moons clone retains inherited discount", async () =>
{
    var f = new CombatFixture();
    CardModel card = f.Card("TwoMoonsDeepIntoTheForest");
    card.EnergyCost.AddThisCombat(-2);
    CardModel clone = card.CreateClone();
    await clone.AfterCardEnteredCombat(clone);
    Equal(5, clone.EnergyCost.GetWithModifiers(CostModifiers.Local));
});

await Check("Two Moons preserves another effect's zero cost", async () =>
{
    var f = new CombatFixture();
    CardModel card = f.Card("TwoMoonsDeepIntoTheForest");
    card.EnergyCost.SetThisCombat(0);
    await card.AfterCardEnteredCombat(card);
    Equal(0, CombatFixture.Cost(card));
});

await Check("Two Moons counts repeated Songs, excludes allies and non-Songs, restores late cards", async () =>
{
    var f = new CombatFixture("Togawasakiko", "Togawasakiko");
    CardModel moon = f.Card("TwoMoonsDeepIntoTheForest");
    await CardPileCmd.Add(moon, PileType.Hand);
    CardModel song = f.Card("Ether");
    await f.Finish(song);
    Equal(6, CombatFixture.Cost(moon));
    await f.Finish(song);
    Equal(5, CombatFixture.Cost(moon));
    await f.Finish(f.Card("Ether", 1));
    await f.Finish(f.Card("Slander"));
    Equal(5, CombatFixture.Cost(moon));
    CardModel late = f.Card("TwoMoonsDeepIntoTheForest");
    await CardPileCmd.Add(late, PileType.Hand);
    Equal(5, CombatFixture.Cost(late));
    CardModel clone = moon.CreateClone();
    await CardPileCmd.Add(clone, PileType.Hand);
    Equal(5, CombatFixture.Cost(clone));
    await f.Finish(song);
    Equal(4, CombatFixture.Cost(moon));
    Equal(4, CombatFixture.Cost(clone));
    Equal(4, CombatFixture.Cost(late));
    clone.EnergyCost.AddThisCombat(-2);
    Equal(2, CombatFixture.Cost(clone));
    Equal(4, CombatFixture.Cost(moon));
    Equal(7, CombatFixture.Cost(CombatFixture.Canonical<CardModel>("TwoMoonsDeepIntoTheForest")));
});

await Check("New combat resets Two Moons and accepts the same Song", async () =>
{
    var f = new CombatFixture();
    CardModel deck = f.Run.CreateCard(CombatFixture.Canonical<CardModel>("TwoMoonsDeepIntoTheForest"), f.Players[0]);
    await CardPileCmd.Add(deck, PileType.Deck);
    f.StartCombat();
    CardModel first = f.Players[0].PlayerCombatState!.DrawPile.Cards.Single(c => c.Id == deck.Id);
    await f.Finish(f.Card("Ether"));
    Equal(6, CombatFixture.Cost(first));
    Equal(7, CombatFixture.Cost(deck));
    CardModel clone = first.CreateClone();
    clone.EnergyCost.AddThisCombat(-3);
    Equal(3, CombatFixture.Cost(clone));
    Equal(6, CombatFixture.Cost(first));
    Equal(7, CombatFixture.Cost(deck));
    f.StartCombat();
    CardModel second = f.Players[0].PlayerCombatState!.DrawPile.Cards.Single(c => c.Id == deck.Id);
    Equal(7, CombatFixture.Cost(second));
    await f.Finish(f.Card("Ether"));
    Equal(6, CombatFixture.Cost(second));
    Equal(7, CombatFixture.Cost(deck));
});

foreach (string sourceName in new[] { "Angles", "SymbolIi", "Sophie" })
{
    await Check(sourceName + " OnPlay redeems Inferiority exactly once", async () =>
    {
        var f = new CombatFixture();
        f.Pressure(3);
        await f.PlayEffect(f.Card(sourceName));
        Equal(2, f.PressureAmount);
        Equal(1, f.HandCount(0, "OVERWORK_ANXIETY"));
        Equal(0, f.Players[0].Deck.Cards.Count(c => c.Id.Entry == "OVERWORK_ANXIETY"));
    });
    await Check(sourceName + " blocked by Artifact never spends pressure", async () =>
    {
        var f = new CombatFixture();
        f.Pressure(3);
        ModelDb.Power<ArtifactPower>().ToMutable().ApplyInternal(f.Enemy, 1, true);
        await f.PlayEffect(f.Card(sourceName));
        Equal(3, f.PressureAmount);
        Equal(0, f.HandCount(0, "OVERWORK_ANXIETY"));
    });
}

await Check("Inferiority repeat redeems again; zero and negative changes do not", async () =>
{
    var f = new CombatFixture();
    f.Pressure(3);
    await f.ApplyInferiority(f.Card("Angles"));
    await f.ApplyInferiority(f.Card("Angles"));
    Equal(1, f.PressureAmount);
    Equal(2, f.HandCount(0, "OVERWORK_ANXIETY"));
    PowerModel power = f.Enemy.Powers.Single(p => p.Id.Entry == "INFERIORITY_POWER");
    await PowerCmd.ModifyAmount(f.Choice, power, -1, null, null);
    await PowerCmd.ModifyAmount(f.Choice, power, 0, null, null);
    Equal(1, f.PressureAmount);
    Equal(2, f.HandCount(0, "OVERWORK_ANXIETY"));
});

await Check("Inferiority without pressure grants no token", async () =>
{
    var f = new CombatFixture();
    await f.ApplyInferiority(f.Card("Angles"));
    Equal(0, f.PressureAmount);
    Equal(0, f.HandCount(0, "OVERWORK_ANXIETY"));
});

foreach (bool conflictingApplier in new[] { false, true })
{
    await Check($"Two Sakikos: source owner wins, conflicting applier={conflictingApplier}", async () =>
    {
        var f = new CombatFixture("Togawasakiko", "Togawasakiko");
        f.Pressure(3);
        await f.ApplyInferiority(f.Card("Angles", 1), conflictingApplier ? f.Players[0].Creature : null);
        Equal(2, f.PressureAmount);
        Equal(0, f.HandCount(0, "OVERWORK_ANXIETY"));
        Equal(1, f.HandCount(1, "OVERWORK_ANXIETY"));
    });
}

await Check("Source-less Inferiority uses Sakiko applier", async () =>
{
    var f = new CombatFixture("Togawasakiko", "Togawasakiko");
    f.Pressure(1);
    await f.ApplyInferiority(null, f.Players[1].Creature);
    Equal(0, f.PressureAmount);
    Equal(0, f.HandCount(0, "OVERWORK_ANXIETY"));
    Equal(1, f.HandCount(1, "OVERWORK_ANXIETY"));
});

await Check("Non-Sakiko source deterministically uses first living Sakiko", async () =>
{
    var f = new CombatFixture("Silent", "Togawasakiko", "Togawasakiko");
    f.Pressure(1);
    await f.ApplyInferiority(f.Card("Angles", 0));
    Equal(0, f.PressureAmount);
    Equal(0, f.HandCount(0, "OVERWORK_ANXIETY"));
    Equal(1, f.HandCount(1, "OVERWORK_ANXIETY"));
    Equal(0, f.HandCount(2, "OVERWORK_ANXIETY"));
});

await Check("Weak, Vulnerable and negative attributes retain single-owner redemption", async () =>
{
    var f = new CombatFixture("Togawasakiko", "Togawasakiko");
    f.Pressure(8);
    CardModel source = f.Card("Angles", 1);
    await PowerCmd.Apply<WeakPower>(f.Choice, f.Enemy, 1, source.Owner.Creature, source);
    await PowerCmd.Apply<VulnerablePower>(f.Choice, f.Enemy, 1, source.Owner.Creature, source);
    await PowerCmd.Apply<StrengthPower>(f.Choice, f.Enemy, -1, source.Owner.Creature, source);
    await PowerCmd.Apply<DexterityPower>(f.Choice, f.Enemy, -1, source.Owner.Creature, source);
    Equal(1, f.PressureAmount);
    Equal(0, f.Players[0].PlayerCombatState!.Hand.Cards.Count);
    Equal(1, f.HandCount(1, "PERSONA_DISSOCIATION"));
    Equal(1, f.HandCount(1, "ALL_YOU_THINK_ABOUT_IS_YOURSELF"));
    Equal(2, f.HandCount(1, "SOCIAL_WITHDRAWAL"));
});

await Check("Special relic registration yields each relic once", () =>
{
    Type pool = typeof(TogawasakikoMod).Assembly.GetType("Togawasakiko_in_Slay_the_Spire.TogawasakikoSpecialRelicPool", true)!;
    foreach (string name in new[] { "UpgradedDollMask", "PianoOfMom" })
        ModHelper.AddModelToPool(pool, typeof(TogawasakikoMod).Assembly.GetType("Togawasakiko_in_Slay_the_Spire." + name, true)!);
    RelicModel[] relics = ModelDb.GetById<RelicPoolModel>(ModelDb.GetId(pool)).AllRelics.ToArray();
    Equal(2, relics.Length);
    Equal(1, relics.Count(r => r.Id.Entry == "UPGRADED_DOLL_MASK"));
    Equal(1, relics.Count(r => r.Id.Entry == "PIANO_OF_MOM"));
    return Task.CompletedTask;
});

await BridgeCardRegression.Run(Check);
await PressureCardRegression.Run(Check);
await SongExpansionRegression.Run(Check);
await OctagramReplayRegression.Run(Check);
await CastAnimationRegression.Run(Check);
await AutoPlayEligibilityRegression.Run(Check);
await MagneticForceRegression.Run(Check);
await EnemyTargetRegression.Run(Check);
await PianoEventRegression.Run(Check);
await ShadowMigrationRegression.Run(Check);

Console.WriteLine($"RESULT {passed} passed, {failures} failed");
Environment.ExitCode = failures == 0 ? 0 : 1;
