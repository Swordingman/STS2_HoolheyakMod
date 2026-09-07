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
public class Tamper : HoolheyakBaseCard
{
    public Tamper() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(8, ValueProp.Move)
    ];

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (!ReferenceEquals(card, this) || Owner?.Creature == null)
            return playCount;

        var creature = Owner.Creature;
        var meander = creature.GetPower<MeanderPower>();
        if (meander == null)
            return playCount;

        if (!WillGainMeanderStack())
            return playCount;

        // 预判这张技能牌打出后是否会触发逶迤，触发则由原版按整张牌重放一次。
        if (meander.Progress + 1 >= MeanderPower.GetThreshold(creature))
            return playCount + 1;

        return playCount;
    }

    private bool WillGainMeanderStack()
    {
        bool willGain = Type == CardType.Skill;
        if (Owner?.Creature?.GetPower<QuincunxPower>() != null)
            willGain = Type == CardType.Attack;
        return willGain;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 8 点格挡；是否额外重放由 ModifyCardPlayCount 预判。
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        // 格挡 8 -> 11
        DynamicVars.Block.UpgradeValueBy(3);
    }
}
