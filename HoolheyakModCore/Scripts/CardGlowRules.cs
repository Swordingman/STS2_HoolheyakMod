using System.Linq;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Scripts.Cards;

/// <summary>只读取状态；出牌判定、颜色及手牌刷新交给游戏原生高亮系统。</summary>
internal static class CardGlowRules
{
    public static bool CanTriggerSlough(CardModel card) =>
        card.IsMutable && card.Owner?.Creature is { } owner && card.Pile?.Type == PileType.Hand &&
        owner.CombatState != null && SloughMechanics.Active(owner) &&
        card.Keywords.Contains(HoolheyakKeywords.Slough);

    public static bool HasReadyBonus(CardModel card)
    {
        if (!card.IsMutable || card.Owner?.Creature is not { } owner ||
            card.Pile?.Type != PileType.Hand || owner.CombatState is not { } combat)
            return false;

        bool slough = SloughMechanics.Active(owner);
        if (slough && (card.Keywords.Contains(HoolheyakKeywords.Slough) || card is PutrescentGermination))
            return true;

        // 只高亮已满足的条件重放；固定重放或危机累加不会让黄圈常驻。
        if (card is HoolheyakBaseCard { ConditionalReplayCount: > 0 })
            return true;

        if (card.Type is CardType.Attack or CardType.Skill)
        {
            if (card.Type == CardType.Skill && owner.GetPower<NextSkillReplayPower>() is { Amount: > 0 })
                return true;
            if (owner.GetPower<InheritedEndPower>()?.WillTrigger(card) == true)
                return true;

            if (owner.GetPower<EruditionPower>()?.WillTrigger(card) == true ||
                owner.GetPower<MeanderPower>()?.WillTrigger(card) == true)
                return true;
        }

        if (card is not (Coercion or Divergence or Refutation or TwinExperiment))
            return false;
        var enemies = combat.GetOpponentsOf(owner).Where(enemy => enemy.IsAlive);
        return card switch
        {
            Coercion => enemies.Any(enemy => enemy.GetPower<LevitatePower>() != null),
            Divergence => enemies.Any(enemy => enemy.GetPower<LiftPower>() is { Amount: > 0 }),
            Refutation => enemies.Any(enemy => Refutation.GetReducibleBonuses(enemy).Count > 0),
            TwinExperiment => enemies.Take(2).Count() == 2,
            _ => false
        };
    }
}
