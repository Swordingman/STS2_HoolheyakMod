using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
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
public class Footnote : HoolheyakBaseCard
{
    public Footnote() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(8, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 8 点格挡
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);

        // 检索牌库/弃牌堆中的第一张技能牌加入手牌（近似实现）
        var player = Owner;
        if (player != null)
        {
            var drawPile = PileType.Draw.GetPile(player);
            var skill = drawPile.Cards.FirstOrDefault(c => c.Type == CardType.Skill);

            if (skill == null)
            {
                var discardPile = PileType.Discard.GetPile(player);
                skill = discardPile.Cards.FirstOrDefault(c => c.Type == CardType.Skill);
            }

            if (skill != null)
            {
                await CardPileCmd.Add(skill, PileType.Hand);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 格挡 8 -> 11
        DynamicVars.Block.UpgradeValueBy(3);
    }
}
