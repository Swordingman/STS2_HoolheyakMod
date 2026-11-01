using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Patches;

[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Remove), new[] { typeof(PowerModel) })]
internal static class EcdysisRemovalPatch
{
    private static void Prefix(PowerModel power, out Creature? __state)
    {
        var owner = power?.Owner;
        __state = power != null && owner != null && owner.Powers.Contains(power) &&
            power.TypeForCurrentAmount == PowerType.Debuff ? owner : null;
    }

    private static void Postfix(PowerModel power, Creature? __state, ref Task __result)
    {
        if (power == null || __state == null) return;
        __result = AfterRemoval(__result, power, __state);
    }

    internal static async Task AfterRemoval(Task original, PowerModel power, Creature creature)
    {
        await original;
        if (creature.Powers.Contains(power)) return;
        if (creature.GetPower<EcdysisStrengthPower>() is { } strength)
            await strength.OnDebuffRemoved();
        if (creature.GetPower<EcdysisDexterityPower>() is { } dexterity)
            await dexterity.OnDebuffRemoved();
    }
}

// Negative Strength/Dexterity become Buffs again at zero, before Remove runs.
// Remember their debuff classification before the amount change instead.
[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.ModifyAmount))]
internal static class EcdysisAmountRemovalPatch
{
    private static void Prefix(PowerModel power, out Creature? __state)
    {
        var owner = power?.Owner;
        __state = power != null && owner != null && owner.Powers.Contains(power) &&
            power.Type != PowerType.Debuff && power.TypeForCurrentAmount == PowerType.Debuff
            ? owner : null;
    }

    private static void Postfix(PowerModel power, Creature? __state, ref Task<int> __result)
    {
        if (__state != null)
            __result = AfterAmountChanged(__result, power, __state);
    }

    private static async Task<int> AfterAmountChanged(Task<int> original, PowerModel power, Creature owner)
    {
        int result = await original;
        if (power.Amount == 0)
            await EcdysisRemovalPatch.AfterRemoval(Task.CompletedTask, power, owner);
        return result;
    }
}
