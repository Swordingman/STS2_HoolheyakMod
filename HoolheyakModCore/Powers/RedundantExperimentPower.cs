using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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

    private readonly List<CardModel> _trackedCards = [];

    /// <summary>
    /// 由 RedundantExperiment 卡牌调用：登记需要自动打出的卡牌
    /// </summary>
    public void TrackCards(IEnumerable<CardModel> cards)
    {
        _trackedCards.Clear();
        _trackedCards.AddRange(cards);
        UpdateAmount();
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_trackedCards.RemoveAll(c => ReferenceEquals(c, cardPlay.Card)) > 0)
        {
            UpdateAmount();

            if (_trackedCards.Count == 0)
                await PowerCmd.Remove(this);
        }
    }

    public override async Task BeforeSideTurnEndEarly(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        System.Collections.Generic.IEnumerable<Creature> participants)
    {
        if (Owner == null || side != Owner.Side || _trackedCards.Count == 0)
            return;

        // 仍停留在手牌/抽牌堆/弃牌堆的追踪牌，在弃牌前自动打出
        var cardsToPlay = _trackedCards
            .Where(c => c.Pile?.Type is PileType.Hand or PileType.Draw or PileType.Discard)
            .ToList();

        if (cardsToPlay.Count == 0)
            return;

        Flash();

        foreach (var card in cardsToPlay)
        {
            await CardCmd.AutoPlay(choiceContext, card, null);
        }

        _trackedCards.Clear();
        UpdateAmount();
        await PowerCmd.Remove(this);
    }

    private void UpdateAmount()
    {
        SetAmount(_trackedCards.Count);
    }
}
