using BaseLib.Abstracts;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class SextilePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(SextilePower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(SextilePower)}.png";

    public override decimal ModifyPowerAmountGivenMultiplicative(PowerModel power,
        Creature applier, decimal amount, Creature? target, CardModel? cardSource) =>
        applier == Owner && power is LiftPower && amount > 0 ? 0m : 1m;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        var context = new ThrowingPlayerChoiceContext();
        if (Owner.GetPower<EruditionPower>() is { } erudition)
            await erudition.CheckAndTriggerAsync(context);
        if (Owner.GetPower<MeanderPower>() is { } meander)
            await meander.CheckAndTriggerAsync(context);
    }
}
