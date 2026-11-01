using HarmonyLib;
using HoolheyakMod.Scripts.Cards;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace HoolheyakMod.Code.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetEnchantedReplayCount))]
internal static class CanonicalReplayCountPatch
{
    [HarmonyPrefix]
    private static bool Prefix(CardModel __instance, ref int __result)
    {
        if (__instance is not HoolheyakBaseCard card)
            return true;

        int replayCount = __instance.BaseReplayCount + card.CanonicalReplayCount + card.ConditionalReplayCount;

        __result = __instance.Enchantment?.EnchantPlayCount(replayCount) ?? replayCount;
        return false;
    }
}

// These cards explain their conditional replay inline, even when it is inactive.
// Only the description's automatic footer excludes that already-described source.
// Gameplay and replay hover tips continue using the full native replay count.
[HarmonyPatch]
internal static class ConditionalReplayDescriptionPatch
{
    private static MethodBase TargetMethod() =>
        AccessTools.GetDeclaredMethods(typeof(CardModel)).Single(method =>
            method.Name == nameof(CardModel.GetDescriptionForPile) && method.GetParameters().Length == 3);

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var nativeReplayCount = AccessTools.Method(typeof(CardModel), nameof(CardModel.GetEnchantedReplayCount));
        var displayReplayCount = AccessTools.Method(typeof(ConditionalReplayDescriptionPatch), nameof(GetFooterReplayCount));
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(nativeReplayCount))
            {
                instruction.opcode = OpCodes.Call;
                instruction.operand = displayReplayCount;
            }
            yield return instruction;
        }
    }

    private static int GetFooterReplayCount(CardModel card)
    {
        if (card is not (PoweredExoskeleton or Wake or Tamper or Wingblade))
            return card.GetEnchantedReplayCount();

        var custom = (HoolheyakBaseCard)card;
        int replayCount = card.BaseReplayCount + custom.CanonicalReplayCount;
        return card.Enchantment?.EnchantPlayCount(replayCount) ?? replayCount;
    }
}
