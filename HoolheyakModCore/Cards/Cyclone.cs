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
public class Cyclone : HoolheyakBaseCard
{
    public Cyclone() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
        new DynamicVar("Cyclone-Magic", 1m),
        new DynamicVar("Cyclone-Analysis", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;

        // 造成 6 点伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 获得 1 层博览和 1 层解析
        await PowerCmd.Apply<EruditionPower>(choiceContext, Owner.Creature, DynamicVars["Cyclone-Magic"].IntValue, Owner.Creature, this);
        await PowerCmd.Apply<AnalysisPower>(choiceContext, Owner.Creature, DynamicVars["Cyclone-Analysis"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 伤害 6 -> 9
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
