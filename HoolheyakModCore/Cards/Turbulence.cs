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
public class Turbulence : HoolheyakBaseCard
{
    public Turbulence() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3, ValueProp.Move),
        new DynamicVar("Turbulence-Magic", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;

        // 造成 3 点伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 近似：每次打出都生成 1 张临时湍流加入手牌
        var copy = ModelDb.Card<Turbulence>().ToMutable();
        if (IsUpgraded && copy.IsUpgradable)
        {
            copy.UpgradeInternal();
            copy.FinalizeUpgradeInternal();
        }

        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner);
    }

    protected override void OnUpgrade()
    {
        // 生成数量系数 1 -> 2
        DynamicVars["Turbulence-Magic"].UpgradeValueBy(1);
    }
}
