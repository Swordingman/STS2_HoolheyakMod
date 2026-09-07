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
public class Divergence : HoolheyakBaseCard
{
    public Divergence() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(12, ValueProp.Move),
        new DynamicVar("Divergence-Magic", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 12 点基础格挡
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);

        // 消耗目标身上的全部升力，按每层 2 点转化为格挡
        if (cardPlay.Target != null)
        {
            var lift = cardPlay.Target.GetPower<LiftPower>();
            if (lift != null && lift.Amount > 0)
            {
                int extraBlock = lift.Amount * DynamicVars["Divergence-Magic"].IntValue;
                await PowerCmd.Remove(lift);
                await CreatureCmd.GainBlock(Owner.Creature, extraBlock, ValueProp.Move, cardPlay);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 格挡 12 -> 14，每层升力转化 2 -> 3
        DynamicVars.Block.UpgradeValueBy(2);
        DynamicVars["Divergence-Magic"].UpgradeValueBy(1);
    }
}
