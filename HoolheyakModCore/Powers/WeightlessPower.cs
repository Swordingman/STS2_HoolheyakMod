using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class WeightlessPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(WeightlessPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(WeightlessPower)}.png";

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        System.Collections.Generic.IEnumerable<Creature> participants)
    {
        if (Owner == null || side != Owner.Side || Amount <= 0)
            return;

        // 回合结束时层数 -1
        if (Amount <= 1)
        {
            await PowerCmd.Remove(this);
        }
        else
        {
            await PowerCmd.Apply<WeightlessPower>(choiceContext, Owner, -1, Owner, null);
        }
    }
}
