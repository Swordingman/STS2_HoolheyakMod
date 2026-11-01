using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Patches;

[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyBlock))]
internal static class DeconstructionBlockPatch
{
    // Cancel the final gain, after Dexterity, Frail and phase multipliers.
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(Creature __1, IEnumerable<AbstractModel> __6, ref decimal __result) =>
        DeconstructionPower.ReduceBlock(__1, __6, ref __result);
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterModifyingBlockAmount))]
internal static class DeconstructionBlockConsumptionPatch
{
    private static bool Prefix(ICombatState __0, decimal __1, CardModel? __2,
        CardPlay? __3, IEnumerable<AbstractModel> __4, ref Task __result)
    {
        if (!DeconstructionPower.TryTakeReduction(__4, out var reduction))
            return true;

        __result = ConsumeAndContinue(reduction!, __0, __1, __2, __3, __4);
        return false;
    }

    private static async Task ConsumeAndContinue(DeconstructionPower.Reduction reduction,
        ICombatState combat, decimal block, CardModel? source, CardPlay? play,
        IEnumerable<AbstractModel> modifiers)
    {
        // Spend first so a nested gain from another modifier cannot reuse layers.
        await reduction.Power.ConsumeReduction(reduction.Amount, source);
        // The receipt was removed before this call, so this invokes the original hooks.
        await Hook.AfterModifyingBlockAmount(combat, block, source, play, modifiers);
    }
}
