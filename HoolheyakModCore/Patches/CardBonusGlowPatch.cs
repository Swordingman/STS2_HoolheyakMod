using HarmonyLib;
using HoolheyakMod.Scripts.Cards;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.ShouldGlowGold), MethodType.Getter)]
internal static class CardBonusGlowPatch
{
    private static void Postfix(CardModel __instance, ref bool __result)
    {
        if (!__result)
            __result = CardGlowRules.HasReadyBonus(__instance);
    }
}
