using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class CorrectedVariablePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(CorrectedVariablePower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(CorrectedVariablePower)}.png";

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (Owner == null || Amount <= 0)
            return;

        // 抽到状态牌 -> 获得博览
        if (card.Type == CardType.Status)
        {
            Flash();
            await PowerCmd.Apply<EruditionPower>(choiceContext, Owner, Amount, Owner, null);
        }
        // 抽到诅咒牌 -> 获得逶迤
        else if (card.Type == CardType.Curse)
        {
            Flash();
            await PowerCmd.Apply<MeanderPower>(choiceContext, Owner, Amount, Owner, null);
        }
    }
}
