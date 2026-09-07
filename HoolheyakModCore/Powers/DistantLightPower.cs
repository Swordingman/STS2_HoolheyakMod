using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HoolheyakMod.Code.Powers;

public class DistantLightPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(DistantLightPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(DistantLightPower)}.png";

    private int _damageDealtThisTurn;

    public override Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        // 只统计本能力持有者造成的正常攻击伤害；不统计纯生命流失（Unblockable）
        if (dealer == Owner && result.UnblockedDamage > 0 && !props.HasFlag(ValueProp.Unblockable))
        {
            _damageDealtThisTurn += result.UnblockedDamage;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || side != Owner.Side)
            return;

        // 本回合伤害不足 6 时，额外触发 1 次博览与 1 次逶迤
        if (_damageDealtThisTurn < 6)
        {
            Flash();
            await PowerCmd.Apply<EruditionPower>(
                choiceContext,
                Owner,
                EruditionPower.GetThreshold(Owner),
                Owner,
                null);
            await PowerCmd.Apply<MeanderPower>(
                choiceContext,
                Owner,
                MeanderPower.GetThreshold(Owner),
                Owner,
                null);
        }

        _damageDealtThisTurn = 0;
    }
}
