using BaseLib.Abstracts;
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

public sealed class Muelsyse : CustomMonsterModel
{
    public override int MinInitialHp => 50;
    public override int MaxInitialHp => 60;

    public override bool HasDeathSfx => false;

    public override NCreatureVisuals? CreateCustomVisuals() =>
        NodeFactory<NCreatureVisuals>.CreateFromScene("res://HoolheyakMod/scenes/Muelsyse.tscn");

    public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
    {
        var skin = skeleton.GetData().FindSkin("default");
        if (skin != null) skeleton.SetSkin(skin);
        skeleton.SetSlotsToSetupPose();
    }

    public override CreatureAnimator? SetupCustomAnimationStates(MegaSprite controller) =>
        SetupAnimationState(controller, idleName: "Idle", deadName: "Die", attackName: "Attack");

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var attack = new MoveState("ATTACK", AttackMove, new SingleAttackIntent(12));
        attack.FollowUpState = attack;
        return new MonsterMoveStateMachine(new[] { attack }, attack);
    }

    private async Task AttackMove(IReadOnlyList<Creature> targets)
    {
        if (targets.Count == 0) return;
        await DamageCmd.Attack(12).FromMonster(this).Targeting(targets[0])
            .WithAttackerAnim("Attack", 0.5f)
            .WithHitFx(VfxCmd.bluntPath)
            .Execute(null);
    }
}
