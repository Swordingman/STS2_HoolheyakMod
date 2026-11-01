using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class RedundantExperimentPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(RedundantExperimentPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(RedundantExperimentPower)}.png";

    private List<CardModel> _trackedCards = [];
    private HashSet<CardModel> _playingCards = [];

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _trackedCards = new List<CardModel>(_trackedCards);
        _playingCards = [];
    }

    public static async Task ApplyToCards(PlayerChoiceContext context, Player player,
        IEnumerable<CardModel> cards, CardModel source)
    {
        var selected = cards.Distinct().ToList();
        if (selected.Count == 0)
            return;

        var existing = player.Creature.GetPower<RedundantExperimentPower>();
        if (existing != null)
        {
            existing.TrackCards(selected);
            return;
        }

        var power = (RedundantExperimentPower)ModelDb.Power<RedundantExperimentPower>().ToMutable();
        power.TrackCards(selected);
        await PowerCmd.Apply(context, power, player.Creature, power._trackedCards.Count,
            player.Creature, source);
    }

    /// <summary>
    /// 由 RedundantExperiment 卡牌调用：登记需要自动打出的卡牌
    /// </summary>
    public void TrackCards(IEnumerable<CardModel> cards)
    {
        AssertMutable();
        foreach (var card in cards)
            if (!_trackedCards.Any(current => ReferenceEquals(current, card)))
                _trackedCards.Add(card);

        if (Owner != null)
            UpdateAmount();
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (Owner?.Player != card.Owner ||
            !_trackedCards.Any(current => ReferenceEquals(current, card)) || !_playingCards.Add(card))
            return;

        Flash();
        try { await CardCmd.AutoPlay(choiceContext, card, null); }
        finally { _playingCards.Remove(card); }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        System.Collections.Generic.IEnumerable<Creature> participants)
    {
        if (Owner == null || side != Owner.Side)
            return;

        _trackedCards.Clear();
        if (Owner.Powers.Contains(this))
            await PowerCmd.Remove(this);
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        _trackedCards.Clear();
        _playingCards.Clear();
        return Task.CompletedTask;
    }

    private void UpdateAmount()
    {
        SetAmount(_trackedCards.Count);
    }
}
