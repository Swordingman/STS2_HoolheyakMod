using Godot;
using HarmonyLib;
using HoolheyakMod.Scripts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace HoolheyakMod.Code.Patches;

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
internal static class SloughSmokePatch
{
    private static void Postfix(NCard __instance)
        => Attach(__instance);

    internal static void Attach(NCard card)
    {
        if (!GodotObject.IsInstanceValid(card) || !card.IsNodeReady() ||
            card.Model is not { IsMutable: true } model || model.Owner?.Creature.CombatState == null ||
            card.GetNodeOrNull<NSloughSmokeVfx>(NSloughSmokeVfx.NodeName) != null)
            return;
        card.AddChild(new NSloughSmokeVfx { Name = NSloughSmokeVfx.NodeName });
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
internal static class SloughSmokeReadyPatch
{
    private static void Postfix(NCard __instance) =>
        Callable.From(() => SloughSmokePatch.Attach(__instance)).CallDeferred();
}
