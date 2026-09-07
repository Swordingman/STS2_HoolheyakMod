using System;
using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class Wingblade : HoolheyakBaseCard
{
    public Wingblade() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9, ValueProp.Move)
    ];

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (!ReferenceEquals(card, this) || Owner?.Creature == null)
            return playCount;

        var creature = Owner.Creature;
        var erudition = creature.GetPower<EruditionPower>();
        if (erudition == null)
            return playCount;

        if (!WillGainEruditionStack())
            return playCount;

        // 预判这张攻击牌打出后是否会触发博览，触发则由原版按整张牌重放一次。
        if (erudition.Progress + 1 >= EruditionPower.GetThreshold(creature))
            return playCount + 1;

        return playCount;
    }

    private bool WillGainEruditionStack()
    {
        bool willGain = Type == CardType.Attack;
        if (Owner?.Creature?.GetPower<QuincunxPower>() != null)
            willGain = Type == CardType.Skill;
        return willGain;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;

        // 造成 9 点伤害；是否额外重放由 ModifyCardPlayCount 预判。
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // 伤害 9 -> 12
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
