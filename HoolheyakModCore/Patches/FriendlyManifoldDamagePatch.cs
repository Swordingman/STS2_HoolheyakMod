using HarmonyLib;
using HoolheyakMod.Code.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using HoolheyakMod.Code.Monsters;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace HoolheyakMod.Code.Patches;

[HarmonyPatch]
internal static class FriendlyManifoldDamagePatch
{
    internal static MethodBase TargetMethod() => AccessTools.GetDeclaredMethods(typeof(CreatureCmd)).Single(method =>
    {
        var parameters = method.GetParameters();
        return method.Name == nameof(CreatureCmd.Damage) &&
#if STS2_BETA
            parameters.Length == 7 &&
#else
            parameters.Length == 6 &&
#endif
            parameters[1].ParameterType == typeof(IEnumerable<Creature>) &&
            parameters[2].ParameterType == typeof(decimal) && parameters[4].ParameterType == typeof(Creature);
    });

    private static void Prefix(ref IEnumerable<Creature> __1, decimal __2, Creature? __4)
    {
        if (__2 <= 0 || __4?.Side != CombatSide.Enemy) return;
        __1 = __1.Select(target =>
            target.Player?.GetRelic<PureWaterSpriteAssist>()?.Manifold is { IsAlive: true } pet &&
            pet.PetOwner == target.Player && pet.CombatState == target.CombatState ? pet : target).ToArray();
    }
}

// Native pet damage spends the master's Block. This summon has its own Block,
// so bypass that one lookup only within the native damage calculation.
[HarmonyPatch]
internal static class FriendlyManifoldBlockPatch
{
    private static MethodBase TargetMethod()
    {
        var stateMachine = FriendlyManifoldDamagePatch.TargetMethod()
            .GetCustomAttribute<AsyncStateMachineAttribute>()!.StateMachineType;
        return AccessTools.Method(stateMachine, "MoveNext");
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var petOwner = AccessTools.PropertyGetter(typeof(Creature), nameof(Creature.PetOwner));
        var blockOwner = AccessTools.Method(typeof(FriendlyManifoldBlockPatch), nameof(GetBlockOwner));
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(petOwner))
            {
                instruction.opcode = OpCodes.Call;
                instruction.operand = blockOwner;
            }
            yield return instruction;
        }
    }

    private static Player? GetBlockOwner(Creature creature) =>
        creature.Monster is FriendlyManifold ? null : creature.PetOwner;
}
