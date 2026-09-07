using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
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

        // 六合相位：首次挂上时也禁止升力
        if (HasSextile())
        {
            await PowerCmd.Remove(this);
            return;
        }

        // 失重：首次获得的升力翻倍
        if (Owner?.GetPower<WeightlessPower>() != null)
        {
            SetAmount(Amount * 2);
        }

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
            // 六合相位：禁止获得/叠加升力
            if (HasSextile())
            {
                modifiedAmount = 0m;
                return true;
            }

            // 失重：获得的升力翻倍
            if (Owner?.GetPower<WeightlessPower>() != null)
            {
                modifiedAmount = amount * 2;
                return true;
            }
        }

        modifiedAmount = amount;
        return false;
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (!ReferenceEquals(power, this) || _handlingAmountChange || amount <= 0 || HasSextile())
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
        if (Owner == null || Owner.GetPower<LevitatePower>() != null)
            return;

        var gravity = Owner.GetPower<GravityPower>();

        if (gravity == null)
        {
            await PowerCmd.Apply<GravityPower>(choiceContext, Owner, 1, Owner, null);
            return;
        }

        if (gravity.Amount > 0 && Amount >= gravity.Amount)
        {
            await PowerCmd.Apply<LevitatePower>(choiceContext, Owner, 1, Owner, null);
            await PowerCmd.Apply<LiftPower>(choiceContext, Owner, -gravity.Amount, Owner, null);
        }
    }

    private bool HasSextile()
    {
        if (Owner?.CombatState == null)
            return false;

        return Owner.CombatState.PlayerCreatures.Any(c => c.GetPower<SextilePower>() != null);
    }
}
