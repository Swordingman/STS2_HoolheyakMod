using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class Byproduct : HoolheyakBaseCard
{
    public Byproduct() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Byproduct-Magic", 1m)
    ];


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = (ByproductPower)ModelDb.Power<ByproductPower>().ToMutable();
        power.IsUpgraded = IsUpgraded;

        await PowerCmd.Apply(
            choiceContext,
            power,
            Owner.Creature,
            DynamicVars["Byproduct-Magic"].IntValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        // 升级状态由 Power 生成牌逻辑读取；当前 C# 桩暂未实现，保持无操作。
    }
}
