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
public class Shockwave : HoolheyakBaseCard
{
    public Shockwave() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move),
        new DynamicVar("Shockwave-Magic", 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null || Owner == null) return;

        var aliveEnemies = CombatState.Enemies.Where(e => e.IsAlive).ToList();
        var magic = DynamicVars["Shockwave-Magic"].IntValue;

        // 对所有敌人造成 12 点伤害
        foreach (var enemy in aliveEnemies)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCardCompatibility(this, cardPlay)
                .Targeting(enemy)
                .Execute(choiceContext);
        }

        // 给所有存活敌人施加 3 层失重（伤害后重新过滤存活者）
        var survivors = CombatState.Enemies.Where(e => e.IsAlive).ToList();
        foreach (var enemy in survivors)
        {
            await PowerCmd.Apply<WeightlessPower>(choiceContext, enemy, magic, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害 12 -> 15，失重 3 -> 4
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["Shockwave-Magic"].UpgradeValueBy(1);
    }
}
