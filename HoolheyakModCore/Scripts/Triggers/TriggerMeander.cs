using System;
using System.Linq;
using System.Threading.Tasks;
using HoolheyakMod.Scripts.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace HoolheyakMod.Code.Powers;

internal static class TriggerMeander
{
    public static async Task Trigger(PlayerChoiceContext choiceContext, Creature? owner)
    {
        if (owner == null)
            return;

        int analysis = owner.GetPower<AnalysisPower>()?.Amount ?? 0;
        int multiplier = GetLegacyMultiplier(owner);
        int finalDamage = 3 * multiplier;
        int finalLift = multiplier;

        var combatState = owner.CombatState;
        var rng = owner.Player?.RunState.Rng.CombatTargets;

        // 逶迤：根据解析层数攻击对应次数。
        for (int i = 0; i < analysis; i++)
        {
            if (combatState == null)
                break;

            var aliveEnemies = combatState.GetOpponentsOf(owner).Where(e => e != null && !e.IsDead).ToList();
            if (aliveEnemies.Count == 0)
                break;

            var target = rng != null ? rng.NextItem(aliveEnemies) : aliveEnemies[0];
            if (target == null)
                continue;

            await PowerCmd.Apply<LiftPower>(choiceContext, target, finalLift, owner, null);
            
            VfxCmd.PlayOnCreatureCenter(target, VfxCmd.bluntPath);

#if STS2_BETA
            await CreatureCmd.Damage(choiceContext, targets: new[] { target }, finalDamage, ValueProp.Unpowered, owner, null, null);
#else
            await CreatureCmd.Damage(choiceContext, targets: new[] { target }, finalDamage, ValueProp.Unpowered, owner, null);
#endif
        }

        var forbidden = owner.GetPower<ForbiddenKnowledgeMeaPower>();
        if (forbidden != null)
            await forbidden.Trigger(choiceContext);

        var memories = owner.GetPower<InheritedMemoriesPower>();
        if (memories != null)
            await memories.Trigger(choiceContext);

        var covenant = owner.GetPower<CovenantDexterityPower>();
        if (covenant != null)
            await covenant.Trigger(choiceContext);

        if (owner.Player != null)
            await ContingencyPlan.ReturnFromDiscard(choiceContext, owner.Player, false);
    }

    private static int GetLegacyMultiplier(Creature owner)
    {
        var legacy = owner.GetPower<KukulkanLegacyPower>();
        return legacy != null ? (int)Math.Pow(2, legacy.Amount) : 1;
    }
}
