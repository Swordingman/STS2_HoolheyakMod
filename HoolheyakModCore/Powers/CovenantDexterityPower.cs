using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HoolheyakMod.Code.Powers;

public class CovenantDexterityPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(CovenantDexterityPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(CovenantDexterityPower)}.png";

    /// <summary>
    /// 逶迤触发时调用：获得敏捷
    /// </summary>
    public async Task Trigger(PlayerChoiceContext choiceContext)
    {
        if (Owner == null || Amount <= 0)
            return;

        Flash();
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, Amount, Owner, null);
    }
}
