using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HoolheyakMod.Code.Powers;

public class SquarePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(SquarePower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(SquarePower)}.png";

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || cardPlay.Card.Owner?.Creature != Owner)
            return;

        Flash();

        // 自己受到 1 点无视格挡的生命流失
        #if STS2_BETA
        await CreatureCmd.Damage(
            choiceContext,
            targets: new[] { Owner },
            1,
            ValueProp.Unblockable | ValueProp.Unpowered,
            Owner,
            null,
            null);
        #else
        await CreatureCmd.Damage(
            choiceContext,
            targets: new[] { Owner },
            1,
            ValueProp.Unblockable | ValueProp.Unpowered,
            Owner,
            null);
        #endif

        // 给所有存活敌人各施加 1 层升力
        if (Owner.CombatState == null)
            return;

        var enemies = Owner.CombatState
            .GetOpponentsOf(Owner)
            .Where(e => e != null && !e.IsDead)
            .ToList();

        foreach (var enemy in enemies)
        {
            await PowerCmd.Apply<LiftPower>(choiceContext, enemy, 1, Owner, null);
        }
    }
}
