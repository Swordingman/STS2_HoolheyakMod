using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using HoolheyakMod.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class KukulkanLegacyPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(KukulkanLegacyPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(KukulkanLegacyPower)}.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Kukulkan-TotalIncrease", 2m),
        new DynamicVar("Kukulkan-Multiplier", 1m)
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

        int increasePerStack = HoolheyakConfig.CurrentDifficulty == 0 ? 2 : 3;
        DynamicVars["Kukulkan-TotalIncrease"].BaseValue = increasePerStack * Amount;
        DynamicVars["Kukulkan-Multiplier"].BaseValue = (decimal)Math.Pow(2, (int)Amount);
        InvokeDisplayAmountChanged();
    }
}
