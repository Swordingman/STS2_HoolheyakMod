using BaseLib.Abstracts;
using HoolheyakMod.Code.Variables;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using System.Linq;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

public abstract class HoolheyakBaseCard : CustomCardModel
{
    // 自动获取卡图路径
    public override string PortraitPath => $"res://HoolheyakMod/images/cards/{GetType().Name}.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var tip in base.ExtraHoverTips)
                yield return tip;
            if (this is IVariableCard or VariableChoiceCard)
                yield return new HoverTip(
                    new LocString("card_keywords", "HOOLHEYAKMOD-VARIABLE.title"),
                    new LocString("card_keywords", "HOOLHEYAKMOD-VARIABLE.description"), null);
        }
    }

    protected HoolheyakBaseCard(
        int energyCost,
        CardType type,
        CardRarity rarity,
        TargetType targetType,
        bool shouldShowInCardLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // ==========================================
    // 常用工具方法 (Utility Methods)
    // ==========================================

    protected Task PlayCastAnimation()
    {
        var creature = Owner?.Creature;
        return creature != null && creature.IsAlive && creature.CombatState != null
            ? CreatureCmd.TriggerAnim(creature, "Cast", 0.2f)
            : Task.CompletedTask;
    }

    /// <summary>
    /// 判断目标生物是否处于半血或以下状态
    /// </summary>
    protected bool IsHalfHealth(Creature creature)
    {
        return creature.CurrentHp <= (creature.MaxHp / 2f);
    }

    /// <summary>
    /// 获取当前玩家本回合打出的卡牌数量
    /// </summary>
    protected int GetCardsPlayedThisTurn()
    {
        if (Owner == null || CombatState == null) return 0;

        return CombatManager.Instance.History.CardPlaysStarted
            .Count(entry =>
                entry.HappenedThisTurn(CombatState) &&
                entry.CardPlay.Card.Owner == Owner);
    }

    /// <summary>
    /// 快捷施加任意 Power
    /// </summary>
    protected async Task ApplyPower<T>(Creature target, int amount, Creature? source = null)
        where T : PowerModel
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<T>(
            new ThrowingPlayerChoiceContext(),
            target,
            amount,
            source ?? Owner.Creature,
            this);
    }

    /// <summary>
    /// 卡牌自身固有的重放次数。
    /// 会和 BaseReplayCount、附魔提供的重放共同进入原版重放系统。
    /// </summary>
    public virtual int CanonicalReplayCount => 0;

    /// <summary>当前条件提供的重放；条件失效不会移除其他来源增加的重放。</summary>
    public virtual int ConditionalReplayCount => 0;
}
