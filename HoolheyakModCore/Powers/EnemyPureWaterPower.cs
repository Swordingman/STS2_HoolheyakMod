using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using System.Linq;
using System.Threading.Tasks;
using HoolheyakMod.Code.Monsters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HoolheyakMod.Code.Powers;

public class EnemyPureWaterPower : CustomPowerModel
{
    private int _reviveTurns;
    private bool _skipNextReviveTick;
    private bool CanRevive => CombatState.Enemies.Any(c => c.IsAlive && c.Monster is Muelsyse) &&
        CombatState.Enemies.Any(c => c != Owner && c.IsAlive && c.Monster is EnemyManifold);

    public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;
    public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature) =>
        creature != Owner || !CanRevive;
    public override bool ShouldOwnerDeathTriggerFatal() => !CanRevive;

    public override Task AfterDeath(PlayerChoiceContext context, Creature creature,
        bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature == Owner)
        {
            _reviveTurns = !wasRemovalPrevented && CanRevive ? 3 : 0;
            _skipNextReviveTick = _reviveTurns > 0;
            if (_reviveTurns > 0)
                NCombatRoom.Instance?.SetCreatureIsInteractable(Owner, false);
        }
        return Task.CompletedTask;
    }

    public async Task TickRevival()
    {
        if (_reviveTurns <= 0 || Owner.IsAlive) return;
        if (!CanRevive) { _reviveTurns = 0; return; }
        // The first end-of-turn after death has not completed a full waiting turn.
        if (_skipNextReviveTick) { _skipNextReviveTick = false; return; }
        if (--_reviveTurns == 0)
        {
            await CreatureCmd.Heal(Owner, Owner.MaxHp / 2, true);
            NCombatRoom.Instance?.SetCreatureIsInteractable(Owner, true);
            await CreatureCmd.TriggerAnim(Owner, "Idle", 0.1f);
        }
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(EnemyPureWaterPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(EnemyPureWaterPower)}.png";
}
