using System.Linq;
using HarmonyLib;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Patches;

[HarmonyPatch(typeof(CombatState), nameof(CombatState.AddCreature))]
internal static class GravitySpawnPatch
{
    [HarmonyPostfix]
    private static void Postfix(CombatState __instance, Creature creature)
    {
        if (creature.Side == CombatSide.Enemy)
        {
            if (HasHoolheyak(__instance))
                EnsureGravity(creature);

            return;
        }

        if (creature.Player?.Character.CardPool is not HoolheyakModCardPool)
            return;

        foreach (Creature enemy in __instance.Enemies)
            EnsureGravity(enemy);
    }

    private static bool HasHoolheyak(CombatState combatState)
    {
        return combatState.Allies.Any(ally =>
            ally.Player?.Character.CardPool is HoolheyakModCardPool);
    }

    private static void EnsureGravity(Creature enemy)
    {
        if (enemy.GetPower<GravityPower>() != null)
            return;

        int gravityAmount = GravityPower.CalculateGravity(enemy);

        GravityPower gravity = (GravityPower)ModelDb.Power<GravityPower>().ToMutable();
        gravity.Applier = enemy;
        gravity.ApplyInternal(enemy, gravityAmount, silent: true);
    }
}