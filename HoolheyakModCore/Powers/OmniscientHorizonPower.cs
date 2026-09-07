using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class OmniscientHorizonPower : CustomPowerModel
{
    public void FlashIcon() => Flash();

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(OmniscientHorizonPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(OmniscientHorizonPower)}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Omniscient-Reward", 0m)
    ];

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        RefreshDynamicVars();
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (ReferenceEquals(power, this))
            RefreshDynamicVars();
        return Task.CompletedTask;
    }

    private void RefreshDynamicVars()
    {
        if (DynamicVars == null)
            return;
        DynamicVars["Omniscient-Reward"].BaseValue = Amount * 2m;
        InvokeDisplayAmountChanged();
    }
}
