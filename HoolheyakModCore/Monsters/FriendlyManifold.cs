using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Monsters;

public sealed class FriendlyManifold : CustomMonsterModel
{
    public override int MinInitialHp => 16;
    public override int MaxInitialHp => 16;
    public override bool HasDeathSfx => false;
    public override bool ShouldFadeAfterDeath => false;
    public override bool ShouldShowInCompendium => false;
    public override bool IsHealthBarVisible => true;
    public override string CustomVisualPath => "res://HoolheyakMod/scenes/FriendlyManifold.tscn";
    protected override string VisualsPath => CustomVisualPath;

    public override NCreatureVisuals? CreateCustomVisuals() =>
        NodeFactory<NCreatureVisuals>.CreateFromScene(CustomVisualPath);

    public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
    {
        var skin = skeleton.GetData().FindSkin("default");
        if (skin != null) skeleton.SetSkin(skin);
        skeleton.SetSlotsToSetupPose();
    }

    public override CreatureAnimator? SetupCustomAnimationStates(MegaSprite controller)
    {
        var idle = new AnimState("A_Idle", true);
        var animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Attack", new AnimState("A_Attack") { NextState = idle });
        animator.AddAnyState("Hit", new AnimState("A_Stun") { NextState = idle });
        animator.AddAnyState("Revive", new AnimState("A_Start") { NextState = idle });
        animator.AddAnyState("Dead", new AnimState("A_Die") {
            NextState = new AnimState("Start") { NextState = new AnimState("Idle_1", true) }
        });
        return animator;
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var wait = new MoveState("WAIT", _ => Task.CompletedTask);
        var attack = new MoveState("ATTACK", _ => Task.CompletedTask, new MultiAttackIntent(3, 3));
        var defend = new MoveState("DEFEND", _ => Task.CompletedTask, new DefendIntent());
        var buff = new MoveState("BUFF", _ => Task.CompletedTask, new BuffIntent());
        wait.FollowUpState = wait;
        attack.FollowUpState = defend.FollowUpState = buff.FollowUpState = wait;
        return new MonsterMoveStateMachine([wait, attack, defend, buff], wait);
    }

    public void ShowPlannedMove(int move)
    {
        if (NextMove == null) RollMove(CombatState.PlayerCreatures);
        var key = move switch { 0 => "ATTACK", 1 => "DEFEND", 2 => "BUFF", _ => "WAIT" };
        SetMoveImmediate((MoveState)MoveStateMachine!.States[key], true);
    }

    public async Task Attack(PlayerChoiceContext context)
    {
        var attack = DamageCmd.Attack(3).FromMonster(this);
        // FromMonster assumes an enemy and preselects all players. This pet
        // instead needs the generic attacker/opponent path with random hits.
        AccessTools.Field(typeof(AttackCommand), "_combatState").SetValue(attack, null);
        var sourceType = AccessTools.Field(typeof(AttackCommand), "_sourceType");
        sourceType.SetValue(attack, System.Enum.Parse(sourceType.FieldType, "None"));
        await attack.TargetingRandomOpponents(CombatState).WithHitCount(3)
            .WithAttackerAnim("Attack", 0.3f).WithHitFx(VfxCmd.bluntPath).Execute(context);
    }
}
