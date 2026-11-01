using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using HoolheyakMod.Scripts.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HoolheyakMod.Code.Powers;

public class RenewalRhythmPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(RenewalRhythmPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(RenewalRhythmPower)}.png";
    private HashSet<ModelId> _known = [];

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _known = new HashSet<ModelId>(_known);
    }
    private bool _triggering;
    private int _pendingTriggers;

    public override Task AfterApplied(Creature? applier, CardModel? source)
    {
        if (Owner != null)
        {
            foreach (var power in Owner.Powers) _known.Add(power.Id);
            Owner.PowerRemoved += Forget;
            Removed += Detach;
        }
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature creature)
    {
        creature.PowerRemoved -= Forget;
        Removed -= Detach;
        _known.Clear();
        _pendingTriggers = 0;
        return Task.CompletedTask;
    }

    private void Forget(PowerModel power) => _known.Remove(power.Id);

    private void Detach()
    {
        if (Owner != null)
            Owner.PowerRemoved -= Forget;
        _known.Clear();
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext context, PowerModel power,
        decimal amount, Creature? applier, CardModel? source)
    {
        if (Owner?.Player == null || power.Owner != Owner ||
            !Owner.Powers.Contains(power) ||
            !_known.Add(power.Id)) return;
        _pendingTriggers++;
        if (_triggering) return;
        _triggering = true;
        try
        {
            while (_pendingTriggers > 0 && Owner.Powers.Contains(this))
            {
                _pendingTriggers--;
                Flash();
                await CardPileCmd.Draw(context, Amount, Owner.Player);
                await PlayerCmd.GainEnergy(Amount, Owner.Player);
            }
        }
        finally { _triggering = false; }
    }
}
