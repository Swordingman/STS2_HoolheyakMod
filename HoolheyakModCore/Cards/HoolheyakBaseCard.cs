using BaseLib.Abstracts;
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
}
