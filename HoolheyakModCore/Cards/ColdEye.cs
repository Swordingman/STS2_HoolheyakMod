using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class ColdEye : HoolheyakBaseCard
{
    public ColdEye() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("ColdEye-Magic", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayCastAnimation();

        if (Owner == null || Owner.Creature == null) return;

        var creature = Owner.Creature;
        if (creature.CombatState == null) return;

        await PowerCmd.Apply<AnalysisPower>(choiceContext, creature,
            DynamicVars["ColdEye-Magic"].IntValue, creature, this);

        foreach (var target in new[] { creature }.Concat(creature.CombatState.HittableEnemies).ToArray())
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, target, 2, creature, this);
            await PowerCmd.Apply<VulnerablePower>(choiceContext, target, 2, creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ColdEye-Magic"].UpgradeValueBy(1);
    }
}
