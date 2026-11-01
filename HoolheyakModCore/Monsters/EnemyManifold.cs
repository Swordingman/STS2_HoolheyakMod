using BaseLib.Abstracts;
using System.Linq;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using BaseLib.Utils.NodeFactories;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Monsters;

public sealed class EnemyManifold : CustomMonsterModel
{
    public override bool ShouldFadeAfterDeath => false;
    public override int MinInitialHp => 24;
    public override int MaxInitialHp => 24;

    public override bool HasDeathSfx => false;

    public override string CustomVisualPath => "res://HoolheyakMod/scenes/EnemyManifold.tscn";
    protected override string VisualsPath => CustomVisualPath;

    public override NCreatureVisuals? CreateCustomVisuals() =>
        NodeFactory<NCreatureVisuals>.CreateFromScene(CustomVisualPath);

    public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
    {
        var skin = skeleton.GetData().FindSkin("default");
        if (skin != null) skeleton.SetSkin(skin);
        skeleton.SetSlotsToSetupPose();
    }

    public override CreatureAnimator? SetupCustomAnimationStates(MegaSprite controller) =>
        SetupAnimationState(controller, idleName: "A_Idle", deadName: "A_Die", attackName: "A_Attack");

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var attack = new MoveState("ATTACK", AttackMove, new MultiAttackIntent(1, 3));
        var buff = new MoveState("STRENGTHEN", StrengthenMove, new BuffIntent());
        var next = new RandomBranchState("CHOOSE");
        next.AddBranch(attack, 1, () => 50f);
        next.AddBranch(buff, 1, () => 50f);
        attack.FollowUpState = next;
        buff.FollowUpState = next;
        return new MonsterMoveStateMachine([attack, buff, next], next);
    }

    private async Task AttackMove(IReadOnlyList<Creature> targets)
    {
        if (targets.Count == 0) return;
        await DamageCmd.Attack(2).FromMonster(this).WithHitCount(3)
            .WithAttackerAnim("Attack", 0.5f).WithHitFx(VfxCmd.bluntPath).Execute(null);
    }

    private async Task StrengthenMove(IReadOnlyList<Creature> targets)
    {
        foreach (var ally in CombatState.Enemies.Where(c => c.IsAlive).ToArray())
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), ally, 1, Creature, null);
    }
}
