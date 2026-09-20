using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Togawasakiko_in_Slay_the_Spire;

[HarmonyPatch]
internal static class OctagramReplayPatches
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        MethodInfo? wrapper = typeof(CardModel).GetMethod(nameof(CardModel.OnPlayWrapper),
            new[] { typeof(PlayerChoiceContext), typeof(Creature), typeof(bool), typeof(ResourceInfo), typeof(bool) });
        Type? stateMachine = wrapper?.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType;
        return stateMachine?.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            ?? throw new InvalidOperationException("Octagram replay guard: native OnPlayWrapper state machine was not found.");
    }

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
    {
        List<CodeInstruction> code = instructions.ToList();
        Type stateMachine = __originalMethod.DeclaringType!;
        FieldInfo index = RequireField(stateMachine, "<i>5__8", typeof(int));
        FieldInfo count = RequireField(stateMachine, "<playCount>5__5", typeof(int));
        FieldInfo card = RequireField(stateMachine, "<>4__this", typeof(CardModel));
        List<int> matches = new();
        for (int i = 1; i + 5 < code.Count; i++)
        {
            if (code[i - 1].opcode != OpCodes.Stfld || !Equals(code[i - 1].operand, index)
                || code[i].opcode != OpCodes.Ldarg_0
                || code[i + 1].opcode != OpCodes.Ldfld || !Equals(code[i + 1].operand, index)
                || code[i + 2].opcode != OpCodes.Ldarg_0
                || code[i + 3].opcode != OpCodes.Ldfld || !Equals(code[i + 3].operand, count)
                || (code[i + 4].opcode != OpCodes.Blt && code[i + 4].opcode != OpCodes.Blt_S)
                || (code[i + 5].opcode != OpCodes.Leave && code[i + 5].opcode != OpCodes.Leave_S)
                || code[i + 4].operand is not Label loopBody)
                continue;

            int body = code.FindIndex(instruction => instruction.labels.Contains(loopBody));
            if (body < 0 || body + 3 >= i
                || code[body].opcode != OpCodes.Ldloc_1
                || code[body + 1].opcode != OpCodes.Ldarg_0
                || code[body + 2].opcode != OpCodes.Ldfld || !Equals(code[body + 2].operand, index)
                || code[body + 3].opcode != OpCodes.Call
                || code[body + 3].operand is not MethodInfo setter
                || setter.DeclaringType != typeof(CardModel) || setter.Name != "set_CurrentPlayIndex"
                || code.Skip(i + 1).Take(4).Any(instruction => instruction.labels.Count != 0 || instruction.blocks.Count != 0))
                continue;

            matches.Add(i + 4);
        }
        if (matches.Count != 1)
            throw new InvalidOperationException($"Octagram replay guard: expected exactly one validated native replay-loop boundary, found {matches.Count}. Native IL is incompatible.");

        // Only adjust the comparison operand. Its original leave/finally and all
        // post-loop pile, cost and choice-context cleanup remain untouched.
        code.InsertRange(matches[0], new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldfld, card),
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldfld, index),
            new CodeInstruction(OpCodes.Call, typeof(OctagramReplayPatches).GetMethod(nameof(CountAtBoundary), BindingFlags.Static | BindingFlags.NonPublic)!)
        });
        return code;
    }

    private static FieldInfo RequireField(Type type, string name, Type fieldType)
    {
        FieldInfo? field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field?.FieldType != fieldType)
            throw new InvalidOperationException($"Octagram replay guard: incompatible native state-machine field {name}.");
        return field;
    }

    private static int CountAtBoundary(int playCount, CardModel card, int nextIndex) =>
        nextIndex > 0 && OwnerHasEnded(card) ? 0 : playCount;

    internal static bool OwnerHasEnded(CardModel card) =>
        card.Owner?.Creature is { } owner && ModSupport.GetPower<OctagramDancePower>(owner)?.HasEnded == true;
}

[HarmonyPatch(typeof(CardCmd), nameof(CardCmd.AutoPlay), new[]
{
    typeof(PlayerChoiceContext), typeof(CardModel), typeof(Creature), typeof(AutoPlayType), typeof(bool), typeof(bool)
})]
internal static class OctagramAutoPlayPatches
{
    [HarmonyPrefix]
    private static bool Prefix(PlayerChoiceContext choiceContext, CardModel card, ref Task __result)
    {
        if (!OctagramReplayPatches.OwnerHasEnded(card)) return true;
        // Native autoplay's ShouldPlay fallback only cleans cards already staged in Play.
        __result = card.Pile?.Type == PileType.Play
            ? card.MoveToResultPileWithoutPlaying(choiceContext)
            : Task.CompletedTask;
        return false;
    }
}
