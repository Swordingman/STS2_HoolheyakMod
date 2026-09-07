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
public class ParameterCorrection : HoolheyakBaseCard
{
    public ParameterCorrection() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Retain];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("ParameterCorrection-Magic", 1m)
    ];

    public override bool HasTurnEndInHandEffect => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 打出时本身没有即时效果
        await Task.CompletedTask;
    }

    /// <summary>
    /// 回合结束时若在手牌中，获得 1 层逶迤（近似原版 RetainCardsAction + MeanderPower）。
    /// </summary>
    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        if (Owner?.Creature == null) return;

        await PowerCmd.Apply<MeanderPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 保留手牌数量 1 -> 2
        DynamicVars["ParameterCorrection-Magic"].UpgradeValueBy(1);
    }
}
