using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Scripts.Utils;
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
public class MadWords : HoolheyakBaseCard
{
    public MadWords() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    // 基础重放 2（总共打出 3 次），升级后重放 3（总共打出 4 次）。
    public override int CanonicalReplayCount => IsUpgraded ? 3 : 2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        // HARD(2) 难度下基础伤害为 5，其余为 6
        new DamageVar(HoolheyakConfig.CurrentDifficulty == 2 ? 5m : 6m, ValueProp.Move),
        new DynamicVar("MadWords-Lift", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(target)
            .Execute(choiceContext);

        await PowerCmd.Apply<LiftPower>(choiceContext, target, DynamicVars["MadWords-Lift"].IntValue, Owner.Creature, this);
    }

}
