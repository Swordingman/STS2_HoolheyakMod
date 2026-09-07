using System.Linq;
using HarmonyLib;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;

namespace HoolheyakMod.Code.Patches;

[HarmonyPatch(typeof(CombatState), nameof(CombatState.AddCreature))]
internal static class GravitySpawnPatch
{
    [HarmonyPostfix]
    private static void Postfix(CombatState __instance, Creature creature)
    {
        if (!creature.IsMonster)
            return;

        if (!__instance.RunState.Players.Any(p => p.Character is Code.Character.HoolheyakMod))
            return;

        if (creature.HasPower<GravityPower>())
            return;

        int gravity = GravityPower.CalculateGravity(creature);
        var ctx = new ThrowingPlayerChoiceContext();

        TaskHelper.RunSafely(PowerCmd.Apply<GravityPower>(ctx, creature, gravity, null, null, silent: true));
    }
}