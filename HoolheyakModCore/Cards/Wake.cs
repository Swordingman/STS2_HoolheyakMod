using System;
using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Combat;
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
public class Wake : HoolheyakBaseCard
{
    public Wake() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(5, ValueProp.Move),
        new DynamicVar("Wake-Magic", 1m)
    ];

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (!ReferenceEquals(card, this) || Owner == null || CombatState == null)
            return playCount;

        var lastPlay = CombatManager.Instance.History.CardPlaysStarted
            .Where(entry => entry.CardPlay.Card.Owner == Owner)
            .LastOrDefault();

        // ModifyCardPlayCount 在本次出牌写入历史前调用，因此最后一条就是上一张牌。
        if (lastPlay?.CardPlay.Card.Type == CardType.Skill)
            return playCount + 1;

        return playCount;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 5 点格挡和 1 层解析；若上一张是技能牌，由 ModifyCardPlayCount 让整张牌重放一次。
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);
        await PowerCmd.Apply<AnalysisPower>(choiceContext, Owner.Creature, DynamicVars["Wake-Magic"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 格挡 5 -> 7
        DynamicVars.Block.UpgradeValueBy(2);
    }
}
