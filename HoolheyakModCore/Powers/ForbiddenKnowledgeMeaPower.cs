using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HoolheyakMod.Code.Powers;

public class ForbiddenKnowledgeMeaPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(ForbiddenKnowledgeMeaPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(ForbiddenKnowledgeMeaPower)}.png";

    /// <summary>
    /// 逶迤触发时调用：对所有敌人造成无视力量加成的伤害
    /// </summary>
    public async Task Trigger(PlayerChoiceContext choiceContext)
    {
        if (Owner == null || Owner.CombatState == null || Amount <= 0)
            return;

        Flash();

        var enemies = Owner.CombatState
            .GetOpponentsOf(Owner)
            .Where(e => e != null && !e.IsDead)
            .ToList();

        if (enemies.Count == 0)
            return;

        #if STS2_BETA
        await CreatureCmd.Damage(
            choiceContext,
            enemies,
            Amount,
            ValueProp.Unpowered,
            Owner,
            null,
            null);
        #else
        await CreatureCmd.Damage(
            choiceContext,
            enemies,
            Amount,
            ValueProp.Unpowered,
            Owner,
            null);
        #endif
    }
}
