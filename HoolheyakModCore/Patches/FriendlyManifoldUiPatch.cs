using Godot;
using HarmonyLib;
using HoolheyakMod.Code.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Patches;

[HarmonyPatch(typeof(NCombatRoom), "PositionPlayersAndPets")]
internal static class FriendlyManifoldLayoutPatch
{
    // PositionPlayersAndPets is static. Its node list is the reliable input
    // during CreateAllyNodes, before the room's initialization has completed.
    private static void Postfix(List<NCreature> __0) => Apply(__0);

    internal static void Apply(IEnumerable<NCreature>? creatureNodes)
    {
        if (creatureNodes == null) return;
        var nodes = creatureNodes.Where(n => n?.Entity != null).ToArray();
        foreach (var node in nodes.Where(n => n.Entity.Monster is FriendlyManifold))
        {
            var owner = node.Entity.PetOwner;
            var ownerNode = owner == null ? null : nodes.FirstOrDefault(n => n.Entity == owner.Creature);
            if (ownerNode == null || node.Visuals == null) continue;
            node.Position = ownerNode.Position + new Vector2(280, 0);
            node.Visuals.Modulate = Colors.White;
            node.ToggleIsInteractable(node.Entity.IsAlive);
        }
    }
}

[HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.AddCreature))]
internal static class FriendlyManifoldAddedLayoutPatch
{
    private static void Postfix(NCombatRoom __instance) =>
        FriendlyManifoldLayoutPatch.Apply(__instance?.CreatureNodes);
}

[HarmonyPatch(typeof(NCreature), nameof(NCreature.RefreshIntents))]
internal static class FriendlyManifoldIntentPatch
{
    private static bool Prefix(NCreature __instance, ref Task __result)
    {
        if (__instance.Entity.Monster is not FriendlyManifold) return true;
        __result = Refresh(__instance);
        return false;
    }

    private static async Task Refresh(NCreature node)
    {
        await node.UpdateIntent(node.Entity.CombatState!.HittableEnemies);
        (AccessTools.Field(typeof(NCreature), "_intentFadeTween").GetValue(node) as Tween)?.Kill();
        node.IntentContainer.Visible = node.Entity.IsAlive;
        node.IntentContainer.Modulate = Colors.White;
    }
}
