using System.Threading.Tasks;
using BaseLib.Abstracts;
using Godot;
using HoolheyakMod.Code.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace HoolheyakMod.Code.Powers;

public class LevitatePower : CustomPowerModel
{
    private Tween? _levitateTween;
    private Node2D? _levitateBody;
    private Vector2 _originalPosition;
    private Vector2 _originalScale;
    private float _originalRotation;
    private Vector2 _hoverCenter;

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.None;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(LevitatePower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(LevitatePower)}.png";

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Owner == null)
            return;

        var ctx = new ThrowingPlayerChoiceContext();
        var source = applier ?? Owner;

        StartLevitateVisual();

        // 进入浮空时移除所有格挡
        if (Owner.Block > 0)
        {
            #if STS2_BETA
            await CreatureCmd.LoseBlock(ctx, Owner, Owner.Block, source);
            #else
            await CreatureCmd.LoseBlock(Owner, Owner.Block);
            #endif
        }

        // 施加 20 层解构
        await PowerCmd.Apply<DeconstructionPower>(ctx, Owner, 20, source, null);

        // 全知视界联动：每层抽 2 张牌并回复 2 点能量
        var horizon = source.GetPower<OmniscientHorizonPower>();
        if (horizon != null && source.Player != null)
        {
            horizon.FlashIcon();
            int bonus = (int)horizon.Amount * 2;

            if (bonus > 0)
            {
                await CardPileCmd.Draw(ctx, bonus, source.Player);
                await PlayerCmd.GainEnergy(bonus, source.Player);
            }
        }

        // WeatherBalloon 遗物联动：玩家拥有热气球时，敌人进入浮空会额外获得升力
        if (Owner.CombatState != null)
        {
            foreach (var playerCreature in Owner.CombatState.PlayerCreatures)
            {
                var balloon = playerCreature.Player?.GetRelic<WeatherBalloon>();
                if (balloon != null)
                    await balloon.TriggerOnEnemyLevitate(Owner);
            }
        }
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        StopLevitateVisual();
        return Task.CompletedTask;
    }

    #if STS2_BETA
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    { return ModifyDamageMultiplicativeCore(target, props, dealer); }
    #else
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    { return ModifyDamageMultiplicativeCore(target, props, dealer); }
    #endif

    private decimal ModifyDamageMultiplicativeCore(Creature? target, ValueProp props, Creature? dealer)
    {
        if (props.HasFlag(ValueProp.Unblockable))
            return 1.0m;

        // 受到伤害增加 50%
        if (target == Owner)
            return 1.5m;

        // 造成伤害减少 70%
        if (dealer == Owner)
            return 0.3m;

        return 1.0m;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Combat.CombatSide side, System.Collections.Generic.IEnumerable<Creature> participants)
    {
        if (Owner == null || side != Owner.Side)
            return;

        await TriggerFallAsync(choiceContext);
    }

    /// <summary>
    /// 开始浮空视觉效果：抬升后持续上下漂浮、左右摆动并轻微旋转
    /// </summary>
    private void StartLevitateVisual()
    {
        if (Owner == null || !Owner.IsMonster)
            return;

        var creatureNode = NCombatRoom.Instance?.GetCreatureNode(Owner);
        if (creatureNode == null)
            return;

        StopLevitateVisual();

        _levitateBody = creatureNode.Body;
        if (_levitateBody == null || !GodotObject.IsInstanceValid(_levitateBody))
            return;

        _originalPosition = _levitateBody.Position;
        _originalScale = _levitateBody.Scale;
        _originalRotation = _levitateBody.Rotation;
        _hoverCenter = _originalPosition + new Vector2(0f, -150f);

        _levitateTween = _levitateBody.CreateTween();

        _levitateTween.TweenProperty(_levitateBody, "position", _hoverCenter, 0.32f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        _levitateTween.Parallel().TweenProperty(_levitateBody, "rotation", _originalRotation + Mathf.DegToRad(1.5f), 0.32f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);

        _levitateTween.TweenCallback(Callable.From(StartLevitateLoop));
    }

    /// <summary>
    /// 持续浮空循环
    /// </summary>
    private void StartLevitateLoop()
    {
        if (_levitateBody == null || !GodotObject.IsInstanceValid(_levitateBody))
            return;

        float rotation = Mathf.DegToRad(3.2f);

        _levitateTween = _levitateBody.CreateTween().SetLoops();

        _levitateTween.TweenProperty(_levitateBody, "position", _hoverCenter + new Vector2(4f, -10f), 0.95f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        _levitateTween.Parallel().TweenProperty(_levitateBody, "rotation", _originalRotation + rotation, 0.95f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        _levitateTween.TweenProperty(_levitateBody, "position", _hoverCenter + new Vector2(-4f, 10f), 1.15f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        _levitateTween.Parallel().TweenProperty(_levitateBody, "rotation", _originalRotation - rotation, 1.15f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    /// <summary>
    /// 播放快速摔落和落地回弹
    /// </summary>
    private async Task PlayFallVisualAsync()
    {
        if (_levitateBody == null || !GodotObject.IsInstanceValid(_levitateBody))
            return;

        _levitateTween?.Kill();
        _levitateTween = null;

        var body = _levitateBody;

        var fallTween = body.CreateTween();

        fallTween.TweenProperty(body, "position", _originalPosition, 0.18f)
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.In);

        fallTween.Parallel().TweenProperty(body, "rotation", _originalRotation, 0.18f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        await body.ToSignal(fallTween, Tween.SignalName.Finished);

        if (!GodotObject.IsInstanceValid(body))
            return;

        var squashScale = new Vector2(_originalScale.X * 1.06f, _originalScale.Y * 0.92f);

        var impactTween = body.CreateTween();

        impactTween.TweenProperty(body, "scale", squashScale, 0.055f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        impactTween.TweenProperty(body, "scale", _originalScale, 0.11f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        await body.ToSignal(impactTween, Tween.SignalName.Finished);

        if (GodotObject.IsInstanceValid(body))
        {
            body.Position = _originalPosition;
            body.Rotation = _originalRotation;
            body.Scale = _originalScale;
        }

        _levitateBody = null;
    }

    /// <summary>
    /// 强制停止浮空视觉并恢复模型
    /// </summary>
    private void StopLevitateVisual()
    {
        _levitateTween?.Kill();
        _levitateTween = null;

        if (_levitateBody != null && GodotObject.IsInstanceValid(_levitateBody))
        {
            _levitateBody.Position = _originalPosition;
            _levitateBody.Rotation = _originalRotation;

            if (_originalScale != Vector2.Zero)
                _levitateBody.Scale = _originalScale;
        }

        _levitateBody = null;
    }

    /// <summary>
    /// 结算摔落伤害并移除浮空/升力
    /// </summary>
    public async Task TriggerFallAsync(PlayerChoiceContext choiceContext)
    {
        if (Owner == null)
            return;

        int gravity = Owner.GetPower<GravityPower>()?.Amount ?? 0;
        int lift = Owner.GetPower<LiftPower>()?.Amount ?? 0;
        int damage = (gravity + lift) * 2;

        Flash();

        // 先让模型真正砸到地面，再出现伤害数字
        await PlayFallVisualAsync();

        if (damage > 0)
        {
            #if STS2_BETA
            await CreatureCmd.Damage(
                choiceContext,
                targets: new[] { Owner },
                damage,
                ValueProp.Unpowered,
                Owner,
                null,
                null);
            #else
            await CreatureCmd.Damage(
                choiceContext,
                targets: new[] { Owner },
                damage,
                ValueProp.Unpowered,
                Owner,
                null);
            #endif
        }

        await PowerCmd.Remove<LevitatePower>(Owner);
        await PowerCmd.Remove<LiftPower>(Owner);
    }
}