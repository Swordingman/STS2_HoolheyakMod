using BaseLib.Abstracts;
using System.Linq;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Combat;
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

public sealed class Muelsyse : CustomMonsterModel
{
    public override int MinInitialHp => 99;
    public override int MaxInitialHp => 99;

    public override bool HasDeathSfx => false;

    public override string CustomVisualPath => "res://HoolheyakMod/scenes/Muelsyse.tscn";
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
        SetupAnimationState(controller, idleName: "Idle", deadName: "Die", attackName: "Attack");

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var summon = new MoveState("SUMMON", SummonMove, new SummonIntent());
        var attack = new MoveState("ATTACK", AttackMove, new MultiAttackIntent(3, 6));
        var buff = new MoveState("STRENGTHEN", StrengthenMove, new BuffIntent(), new DefendIntent());
        var debuff = new MoveState("DEBUFF", DebuffMove, new DebuffIntent());
        var next = new RandomBranchState("CHOOSE");
        next.AddBranch(attack, 1, () => 40f);
        next.AddBranch(buff, 1, () => 30f);
        next.AddBranch(debuff, 1, () => 30f);
        summon.FollowUpState = next;
        attack.FollowUpState = next;
        buff.FollowUpState = next;
        debuff.FollowUpState = next;
        return new MonsterMoveStateMachine([summon, attack, buff, debuff, next], summon);
    }

    private async Task SummonMove(IReadOnlyList<Creature> targets)
    {
        for (int i = 1; i <= 3; i++)
        {
            var minion = CombatState.CreateCreature(ModelDb.Monster<EnemyManifold>().ToMutable(),
                Creature.Side, "water" + i);
            await CreatureCmd.Add(minion);
            await PowerCmd.Apply<EnemyPureWaterPower>(new ThrowingPlayerChoiceContext(),
                minion, 1, Creature, null);
        }
    }

    private async Task AttackMove(IReadOnlyList<Creature> targets)
    {
        if (targets.Count == 0) return;
        await DamageCmd.Attack(3).FromMonster(this)
            .WithHitCount(6).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx(VfxCmd.slashPath).Execute(null);
    }

    private async Task StrengthenMove(IReadOnlyList<Creature> targets)
    {
        foreach (var ally in CombatState.Enemies.Where(c => c.IsAlive).ToArray())
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), ally, 1, Creature, null);
            await CreatureCmd.GainBlock(ally, 12, ValueProp.Move, null);
        }
    }

    private async Task DebuffMove(IReadOnlyList<Creature> targets)
    {
        foreach (var target in targets)
        {
            await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), target, 2, Creature, null);
            await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), target, 2, Creature, null);
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext context, CombatSide side,
        IEnumerable<Creature> creatures)
    {
        if (side != Creature.Side || !Creature.IsAlive) return;
        foreach (var minion in CombatState.Enemies.ToArray())
            if (minion.GetPower<EnemyPureWaterPower>() is { } water)
                await water.TickRevival();
    }

    public override async Task AfterDeath(PlayerChoiceContext context, Creature creature,
        bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature != Creature) return;
        foreach (var minion in CombatState.Enemies.Where(c => c.IsAlive && c.Monster is EnemyManifold).ToArray())
            await CreatureCmd.Kill(minion);
    }
}
