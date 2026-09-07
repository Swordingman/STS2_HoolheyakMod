using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class ForbiddenKnowledgeEruPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(ForbiddenKnowledgeEruPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(ForbiddenKnowledgeEruPower)}.png";

    /// <summary>
    /// 博览触发时调用：抽牌
    /// </summary>
    public async Task Trigger(PlayerChoiceContext choiceContext)
    {
        if (Owner == null || Owner.Player == null || Amount <= 0)
            return;

        Flash();
        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
    }
}
