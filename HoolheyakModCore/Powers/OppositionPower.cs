using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HoolheyakMod.Code.Powers;

public class OppositionPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(OppositionPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(OppositionPower)}.png";

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await RecalculateAllGravityAsync();
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);
        await RecalculateAllGravityAsync();
    }

    private async Task RecalculateAllGravityAsync()
    {
        if (Owner?.CombatState == null)
            return;

        // 对冲降临时，让所有敌方重力重新计算并复查浮空
        var gravityPowers = Owner.CombatState
            .GetOpponentsOf(Owner)
            .Select(e => e.GetPower<GravityPower>())
            .Where(p => p != null)
            .ToList();

        foreach (var gravity in gravityPowers)
        {
            await gravity!.RecalculateAndCheckLevitateAsync(new ThrowingPlayerChoiceContext());
        }
    }

    /// <summary>
    /// 对冲相位下无法获得格挡。
    /// </summary>
    public override decimal ModifyBlockAdditive(
        Creature target,
        decimal block,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target == Owner)
            return 0m;

        return block;
    }
}
