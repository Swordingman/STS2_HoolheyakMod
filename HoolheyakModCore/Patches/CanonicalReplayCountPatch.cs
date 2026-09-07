using HarmonyLib;
using HoolheyakMod.Scripts.Cards;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetEnchantedReplayCount))]
internal static class CanonicalReplayCountPatch
{
    [HarmonyPrefix]
    private static bool Prefix(CardModel __instance, ref int __result)
    {
        if (__instance is not HoolheyakBaseCard card)
            return true;

        int replayCount = __instance.BaseReplayCount + card.CanonicalReplayCount;

        __result = __instance.Enchantment?.EnchantPlayCount(replayCount) ?? replayCount;
        return false;
    }
}