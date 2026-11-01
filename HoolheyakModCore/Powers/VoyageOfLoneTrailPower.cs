using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HoolheyakMod.Code.Powers;

public class VoyageOfLoneTrailPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(VoyageOfLoneTrailPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(VoyageOfLoneTrailPower)}.png";

    internal int AttackDamageThisTurn() => Owner?.CombatState is { } combat
        ? CombatManager.Instance.History.Entries.OfType<DamageReceivedEntry>()
            .Where(entry => entry.HappenedThisTurn(combat) && entry.Dealer == Owner &&
                entry.Receiver.Side != Owner.Side && entry.CardSource?.Type == CardType.Attack)
            .Sum(entry => entry.Result.UnblockedDamage)
        : 0;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || side != Owner.Side)
            return;

        // 本回合伤害不足 6 时，额外触发 1 次博览与 1 次逶迤
        if (AttackDamageThisTurn() < 6)
        {
            Flash();
            for (int i = 0; i < Amount; i++)
            {
                await TriggerMeander.Trigger(choiceContext, Owner);
                await TriggerErudition.Trigger(choiceContext, Owner);
            }
        }

    }
}
