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

public class TemporaryReplayPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(TemporaryReplayPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(TemporaryReplayPower)}.png";
    private List<CardModel> _cards = [];

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _cards = new List<CardModel>(_cards);
    }

    public void Track(CardModel card) => _cards.Add(card);

    public override Task AfterApplied(Creature? applier, CardModel? source)
    {
        Removed += ClearTemporaryReplays;
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        Removed -= ClearTemporaryReplays;
        ClearTemporaryReplays();
        return Task.CompletedTask;
    }

    private void ClearTemporaryReplays()
    {
        foreach (var card in _cards)
            card.BaseReplayCount = Math.Max(0, card.BaseReplayCount - 1);
        _cards.Clear();
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext context, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || side != Owner.Side) return;
        await PowerCmd.Remove(this);
    }
}
