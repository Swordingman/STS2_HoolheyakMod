using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class InheritedEndPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(InheritedEndPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(InheritedEndPower)}.png";

    private CardType? _firstCardType;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || cardPlay.Card.Owner?.Creature != Owner)
            return;

        var type = cardPlay.Card.Type;
        if (type != CardType.Attack && type != CardType.Skill)
            return;

        if (_firstCardType != null && _firstCardType != type)
        {
            Flash();

            if (Owner.Player != null)
            {
                await PlayerCmd.GainEnergy(Amount, Owner.Player);
                await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
            }

            _firstCardType = null;
        }
        else
        {
            _firstCardType = type;
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        System.Collections.Generic.IEnumerable<Creature> participants)
    {
        // 回合结束时移除此能力
        if (Owner != null && side == Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }
}
