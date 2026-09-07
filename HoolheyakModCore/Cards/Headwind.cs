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
public class Headwind : HoolheyakBaseCard
{
    public Headwind() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(2, ValueProp.Move),
        new DynamicVar("Headwind-Hits", 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null || Owner == null) return;

        var rng = Owner.RunState.Rng.CombatTargets;
        int hits = DynamicVars["Headwind-Hits"].IntValue;

        // 随机攻击 3 次，每次对随机存活敌人造成 2 点伤害并施加 1 层升力
        for (int i = 0; i < hits; i++)
        {
            var aliveEnemies = CombatState.Enemies.Where(e => e.IsAlive).ToList();
            if (aliveEnemies.Count == 0) break;

            var target = rng.NextItem(aliveEnemies);
            if (target == null) continue;

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCardCompatibility(this, cardPlay)
                .Targeting(target)
                .Execute(choiceContext);

            await PowerCmd.Apply<LiftPower>(choiceContext, target, 1, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害 2 -> 3
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}
