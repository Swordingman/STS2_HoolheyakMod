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

public class TrinePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(TrinePower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(TrinePower)}.png";

    private bool _justApplied = true;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        // 获得三合相位的当回合立即生效一次
        _justApplied = true;
        await GainRewardAsync(new ThrowingPlayerChoiceContext());
    }

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        // 跨回合后继续生效；刚挂上的当回合不重复触发
        if (Owner != player.Creature || _justApplied)
            return;

        await GainRewardAsync(choiceContext);
    }

    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        System.Collections.Generic.IEnumerable<Creature> participants)
    {
        if (Owner != null && side == Owner.Side)
        {
            _justApplied = false;
        }

        return Task.CompletedTask;
    }

    private async Task GainRewardAsync(PlayerChoiceContext choiceContext)
    {
        if (Owner?.Player == null)
            return;

        Flash();
        await PlayerCmd.GainEnergy(1, Owner.Player);
        await CardPileCmd.Draw(choiceContext, 2, Owner.Player);
    }

    #if STS2_BETA
    public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay, ResourceInfo resources, CardLocation location)
    {
        if (card.Owner?.Creature == Owner && card.Type == CardType.Attack)
        {
            location.pileType = PileType.Exhaust;
            Flash();
        }

        return location;
    }
    #else
    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
    {
        if (card.Owner?.Creature == Owner && card.Type == CardType.Attack)
        {
            Flash();
            return (PileType.Exhaust, position);
        }

        return (pileType, position);
    }
    #endif
}
