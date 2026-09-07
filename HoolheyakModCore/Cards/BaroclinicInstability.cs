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
public class BaroclinicInstability : HoolheyakBaseCard
{
    public BaroclinicInstability() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
        new DynamicVar("Baroclinic-Magic", 4m),
        new DynamicVar("Baroclinic-Lift", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null || Owner == null) return;

        var aliveEnemies = CombatState.Enemies.Where(e => e.IsAlive).ToList();
        var magic = DynamicVars["Baroclinic-Magic"].IntValue;
        var lift = DynamicVars["Baroclinic-Lift"].IntValue;

        // 对所有敌人造成 6 点伤害
        foreach (var enemy in aliveEnemies)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCardCompatibility(this, cardPlay)
                .Targeting(enemy)
                .Execute(choiceContext);
        }

        // 给所有存活敌人施加解构与升力（伤害后重新过滤存活者）
        var survivors = CombatState.Enemies.Where(e => e.IsAlive).ToList();
        foreach (var enemy in survivors)
        {
            await PowerCmd.Apply<DeconstructionPower>(choiceContext, enemy, magic, Owner.Creature, this);
            await PowerCmd.Apply<LiftPower>(choiceContext, enemy, lift, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害 6 -> 8，解构 4 -> 5，升力 1 -> 2
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Baroclinic-Magic"].UpgradeValueBy(1);
        DynamicVars["Baroclinic-Lift"].UpgradeValueBy(1);
    }
}
