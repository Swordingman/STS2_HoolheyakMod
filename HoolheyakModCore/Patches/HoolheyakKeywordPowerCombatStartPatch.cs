using HarmonyLib;
using HoolheyakMod.Code.Character;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

/// <summary>
/// 战斗开始时自动给 Hoolheyak 挂上博览和逶迤。
/// 两个 Power 的内部 Amount 都从 1 开始，该 1 层仅作为隐藏哨兵，真实 Progress 为 0。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
internal static class HoolheyakKeywordPowerCombatStartPatch
{
    [HarmonyPrefix]
    private static void Prefix(CombatState? combatState)
    {
        if (combatState == null)
            return;

        foreach (Creature creature in combatState.Allies)
        {
            if (creature.Player == null)
                continue;

            // 只处理 Hoolheyak 角色。
            if (creature.Player.Character.CardPool is not HoolheyakModCardPool)
                continue;

            EnsurePower<EruditionPower>(creature);
            EnsurePower<MeanderPower>(creature);
        }
    }

    private static void EnsurePower<T>(Creature owner) where T : PowerModel
    {
        if (owner.GetPower<T>() != null)
            return;

        // ToMutable() 返回 PowerModel，因此这里显式转换为 T。
        T power = (T)ModelDb.Power<T>().ToMutable();
        power.Applier = owner;

        // silent = true，避免把内部哨兵当成真正获得的博览 / 逶迤。
        power.ApplyInternal(owner, 1, silent: true);
    }
}
