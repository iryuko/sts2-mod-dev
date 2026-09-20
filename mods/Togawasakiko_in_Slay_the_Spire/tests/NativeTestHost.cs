using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using Togawasakiko_in_Slay_the_Spire;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Commands;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

// The original logger queries Godot before checking TestMode. Only replace
// native environment reads and logging; card, power, history and pile code stays real.
internal static class NativeTestHost
{
    internal static void Initialize()
    {
        var harmony = new Harmony("sakiko.headless-regression");
        harmony.Patch(AccessTools.Method(typeof(OS), nameof(OS.GetCmdlineArgs)),
            prefix: new HarmonyMethod(typeof(NativeTestHost), nameof(CommandLine)));
        harmony.Patch(AccessTools.Method(typeof(OS), nameof(OS.HasFeature)),
            prefix: new HarmonyMethod(typeof(NativeTestHost), nameof(HasFeature)));
        harmony.Patch(AccessTools.Method(typeof(Godot.Time), nameof(Godot.Time.GetTicksMsec)),
            prefix: new HarmonyMethod(typeof(NativeTestHost), nameof(Ticks)));
        harmony.Patch(AccessTools.Method(typeof(ConsoleLogPrinter), nameof(ConsoleLogPrinter.Print)),
            prefix: new HarmonyMethod(typeof(NativeTestHost), nameof(Print)));
        harmony.CreateClassProcessor(typeof(TogawasakikoMod).Assembly.GetType(
            "Togawasakiko_in_Slay_the_Spire.CombatWatcherPatches", true)!).Patch();
        harmony.CreateClassProcessor(typeof(TogawasakikoMod).Assembly.GetType(
            "Togawasakiko_in_Slay_the_Spire.SongFreeXCostPatches", true)!).Patch();
        harmony.CreateClassProcessor(typeof(TogawasakikoMod).Assembly.GetType(
            "Togawasakiko_in_Slay_the_Spire.OctagramReplayPatches", true)!).Patch();
        harmony.CreateClassProcessor(typeof(TogawasakikoMod).Assembly.GetType(
            "Togawasakiko_in_Slay_the_Spire.OctagramAutoPlayPatches", true)!).Patch();
        // GetDistinctForCombat supports this native injector; GetForCombat does not.
        // Control only the randomness boundary for explicitly scripted generation tests.
        harmony.Patch(AccessTools.Method(typeof(CardFactory), nameof(CardFactory.GetForCombat)),
            prefix: new HarmonyMethod(typeof(NativeTestHost), nameof(GenerationOverride)));
        Type shuffle = AccessTools.Method(typeof(CardPileCmd), nameof(CardPileCmd.Shuffle))
            .GetCustomAttribute<AsyncStateMachineAttribute>()!.StateMachineType;
        harmony.Patch(AccessTools.Method(shuffle, "MoveNext"),
            transpiler: new HarmonyMethod(typeof(NativeTestHost), nameof(ShuffleFrameTiming)));
    }

    private static bool CommandLine(ref string[] __result)
    {
        __result = ["--headless"];
        return false;
    }

    private static bool GenerationOverride(ref IEnumerable<CardModel> __result)
    {
        var cards = TestRngInjector.ConsumeCombatCardGenerationOverride();
        if (cards == null) return true;
        __result = cards;
        return false;
    }

    private static IEnumerable<CodeInstruction> ShuffleFrameTiming(IEnumerable<CodeInstruction> instructions)
    {
        var code = instructions.ToList();
        int matches = 0;
        for (int i = 0; i + 3 < code.Count; i++)
        {
            if (!code[i].Calls(AccessTools.Method(typeof(Engine), nameof(Engine.GetMainLoop)))
                || code[i + 1].opcode != OpCodes.Castclass || !Equals(code[i + 1].operand, typeof(SceneTree))
                || !code[i + 2].Calls(AccessTools.PropertyGetter(typeof(SceneTree), nameof(SceneTree.Root)))
                || !code[i + 3].Calls(AccessTools.Method(typeof(Node), nameof(Node.GetProcessDeltaTime)))) continue;
            // Replace only Godot's visual frame clock, not shuffle RNG, moves, or hooks.
            code[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NativeTestHost), nameof(FrameTime)))
                .WithLabels(code[i].labels);
            code.RemoveRange(i + 1, 3);
            matches++;
        }
        if (matches != 1) throw new InvalidOperationException("Native shuffle frame-clock pattern changed.");
        return code;
    }

    private static double FrameTime() => 1.0 / 60.0;

    private static bool HasFeature(ref bool __result)
    {
        __result = false;
        return false;
    }

    private static bool Ticks(ref ulong __result)
    {
        __result = (ulong)System.Environment.TickCount64;
        return false;
    }

    private static bool Print(LogLevel logLevel, string text)
    {
        Console.WriteLine($"[{logLevel}] {text}");
        return false;
    }
}
