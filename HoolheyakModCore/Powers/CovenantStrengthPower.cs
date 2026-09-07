using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HoolheyakMod.Code.Powers;

public class CovenantStrengthPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(CovenantStrengthPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(CovenantStrengthPower)}.png";

    /// <summary>
    /// 博览触发时调用：获得力量
    /// </summary>
    public async Task Trigger(PlayerChoiceContext choiceContext)
    {
        if (Owner == null || Amount <= 0)
            return;

        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);
    }
}
