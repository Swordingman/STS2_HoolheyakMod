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
public class ContingencyPlan : HoolheyakBaseCard
{
    public ContingencyPlan() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 1 点能量
        await PlayerCmd.GainEnergy(1, Owner);
    }

    protected override void OnUpgrade()
    {
        // 无升级数值变化
    }

    /// <summary>
    /// 博览/逶迤触发时，将弃牌堆中的应急方案移回手牌。
    /// </summary>
    public static async Task ReturnFromDiscard(PlayerChoiceContext choiceContext, Player player, bool requireUpgrade)
    {
        var discard = PileType.Discard.GetPile(player);
        var cardsToMove = discard.Cards
            .Where(c => c is ContingencyPlan && (!requireUpgrade || c.IsUpgraded))
            .ToList();

        foreach (var card in cardsToMove)
            await CardPileCmd.Add(card, PileType.Hand);
    }
}
