using System.Threading.Tasks;
using BaseLib.Abstracts;
using HoolheyakMod.Code.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HoolheyakMod.Code.Powers;

public class ComplementaryExperimentPower :
    CustomPowerModel,
    IVariableTriggeredPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath =>
        $"res://HoolheyakMod/images/powers/{nameof(ComplementaryExperimentPower)}.png";

    public override string? CustomBigIconPath =>
        $"res://HoolheyakMod/images/powers/large/{nameof(ComplementaryExperimentPower)}.png";

    public async Task OnVariableTriggered(
        PlayerChoiceContext choiceContext,
        CardModel sourceCard)
    {
        if (Owner == null || Amount <= 0)
            return;

        Flash();

        await CreatureCmd.GainBlock(
            Owner,
            Amount,
            ValueProp.Move,
            null
        );
    }
}