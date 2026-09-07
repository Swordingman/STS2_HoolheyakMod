using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class InheritedMemoriesPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(InheritedMemoriesPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(InheritedMemoriesPower)}.png";

    /// <summary>
    /// 博览/逶迤触发时调用：获得解析
    /// </summary>
    public async Task Trigger(PlayerChoiceContext choiceContext)
    {
        if (Owner == null || Amount <= 0)
            return;

        Flash();
        await PowerCmd.Apply<AnalysisPower>(choiceContext, Owner, Amount, Owner, null);
    }
}
