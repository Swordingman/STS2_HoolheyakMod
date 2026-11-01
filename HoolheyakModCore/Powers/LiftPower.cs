using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class LiftPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(LiftPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(LiftPower)}.png";

    private bool _handlingAmountChange;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Amount <= 0)
            return;

        // 首次施加时还没有升力能力参与接收钩子，在此应用相同的倍率。
        int multiplier = GetLiftMultiplier();
        if (multiplier != 1)
            SetAmount(Amount * multiplier);

        // 首次挂上升力时立即检查浮空
        await CheckLevitateAsync(new ThrowingPlayerChoiceContext());
    }

    public override bool TryModifyPowerAmountReceived(
        PowerModel canonicalPower,
        Creature target,
        decimal amount,
        Creature? applier,
        out decimal modifiedAmount)
    {
        // 只处理“正在给本能力持有者施加升力”的情况
        if (target == Owner && canonicalPower.Id == Id && amount > 0)
        {
            int multiplier = GetLiftMultiplier();
            if (multiplier != 1)
            {
                modifiedAmount = amount * multiplier;
                return true;
            }
        }

        modifiedAmount = amount;
        return false;
    }

    private int GetLiftMultiplier()
    {
        int multiplier = Owner?.GetPower<WeightlessPower>() != null ? 2 : 1;

        // 对冲与失重独立相乘；多位对冲持有者仍只提供一次对冲倍率。
        if (Owner?.Side == CombatSide.Enemy &&
            Owner.CombatState?.PlayerCreatures.Any(c =>
                c.IsAlive && c.GetPower<OppositionPower>() != null) == true)
            multiplier *= 2;

        return multiplier;
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (!ReferenceEquals(power, this) || _handlingAmountChange || amount <= 0)
            return;

        _handlingAmountChange = true;
        try
        {
            await CheckLevitateAsync(choiceContext);
        }
        finally
        {
            _handlingAmountChange = false;
        }
    }

    /// <summary>
    /// 检查是否需要进入浮空状态（公开给重力变化时调用）
    /// </summary>
    public async Task CheckLevitateAsync(PlayerChoiceContext choiceContext)
    {
        if (Owner == null || !Owner.IsAlive || Amount <= 0 || Owner.GetPower<LevitatePower>() != null)
            return;

        var gravity = Owner.GetPower<GravityPower>();

        if (gravity == null)
        {
            await PowerCmd.Apply<GravityPower>(choiceContext, Owner, 1, Owner, null);
            return;
        }

        if (Amount >= gravity.Amount)
        {
            var target = Owner;
            await PowerCmd.Remove(this);
            await PowerCmd.Apply<LevitatePower>(choiceContext, target, 1, target, null);
        }
    }

}
