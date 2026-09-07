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
public class CrossSearch : HoolheyakBaseCard
{
    public CrossSearch() : base(0, CardType.Skill, CardRarity.Common, TargetType.None, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("CrossSearch-Draw", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 抽 2 张牌
        await CardPileCmd.Draw(choiceContext, DynamicVars["CrossSearch-Draw"].IntValue, Owner);

        // 近似：丢弃一张手牌；若为攻击牌获得博览，若为技能牌获得逶迤
        var player = Owner;
        if (player != null)
        {
            var hand = PileType.Hand.GetPile(player);
            var cardToDiscard = hand.Cards.FirstOrDefault();
            if (cardToDiscard != null)
            {
                await CardCmd.Discard(choiceContext, cardToDiscard);

                if (cardToDiscard.Type == CardType.Attack)
                    await PowerCmd.Apply<EruditionPower>(choiceContext, player.Creature, 1, player.Creature, this);
                else if (cardToDiscard.Type == CardType.Skill)
                    await PowerCmd.Apply<MeanderPower>(choiceContext, player.Creature, 1, player.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 抽牌 2 -> 3
        DynamicVars["CrossSearch-Draw"].UpgradeValueBy(1);
    }
}
