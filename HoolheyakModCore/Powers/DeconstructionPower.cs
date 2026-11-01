using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class DeconstructionPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(DeconstructionPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(DeconstructionPower)}.png";

    // ModifyBlock also runs for card previews. The native pipeline creates a fresh
    // modifiers list each time; only an actual gain passes that same list to
    // AfterModifyingBlockAmount. Weak keys discard unused preview receipts.
    private static readonly ConditionalWeakTable<IEnumerable<AbstractModel>, Reduction> Pending = new();

    internal sealed record Reduction(DeconstructionPower Power, int Amount);

    internal static void ReduceBlock(Creature creature, IEnumerable<AbstractModel> modifiers, ref decimal block)
    {
        if (creature.GetPower<DeconstructionPower>() is not { Amount: > 0 } power || block < 1m)
            return;

        int absorbed = (int)Math.Min(power.Amount, decimal.Floor(block));
        block -= absorbed;
        Pending.Add(modifiers, new Reduction(power, absorbed));
    }

    internal static bool TryTakeReduction(IEnumerable<AbstractModel> modifiers, out Reduction? reduction)
    {
        if (!Pending.TryGetValue(modifiers, out reduction))
            return false;
        Pending.Remove(modifiers);
        return true;
    }

    internal async Task ConsumeReduction(int absorbed, CardModel? source)
    {
        if (Owner == null || !Owner.Powers.Contains(this) || Amount <= 0)
            return;

        Flash();
        await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), this,
            -Math.Min(absorbed, Amount), Owner, source);
    }
}
