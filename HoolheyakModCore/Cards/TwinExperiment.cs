using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Code.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class TwinExperiment : HoolheyakBaseCard, IVariableCard
{
    private Creature? _target1;
    private Creature? _target2;

    public TwinExperiment() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Move)
    ];

    public bool CanAllIn => true;
    public bool CanBeAutoTriggered => false;

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay, bool isAutoTriggered = false)
    {
        if (_target1 == null)
            return [];

        return [
            new VariableChoice(async context => {
                var extra = PickExtraTarget(true);
                if (extra != null)
                    await DealExtraDamage(context, cardPlay, extra);
            }),
            new VariableChoice(async context => {
                var extra = PickExtraTarget(false);
                if (extra != null)
                    await DealExtraDamage(context, cardPlay, extra);
            })
        ];
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null || Owner == null) return;

        var aliveEnemies = CombatState.Enemies.Where(e => e.IsAlive).ToList();
        if (aliveEnemies.Count == 0) return;

        var targets = aliveEnemies
            .OrderBy(_ => Owner.RunState.Rng.Shuffle.NextInt(1000000))
            .Take(Math.Min(2, aliveEnemies.Count))
            .ToList();

        _target1 = targets[0];
        _target2 = targets.Count > 1 ? targets[1] : null;

        foreach (var target in targets)
        {
            for (int i = 0; i < 2; i++)
            {
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .FromCardCompatibility(this, cardPlay)
                    .Targeting(target)
                    .Execute(choiceContext);
            }
        }

        if (_target2 != null)
        {
            await VariableCmd.Choose(choiceContext, this, cardPlay);
        }
    }

    private Creature? PickExtraTarget(bool moreHp)
    {
        if (_target1 == null)
            return null;

        bool t1Alive = _target1.IsAlive;
        bool t2Alive = _target2 != null && _target2.IsAlive;

        if (t1Alive && t2Alive && _target2 != null)
        {
            return moreHp
                ? (_target1.CurrentHp >= _target2.CurrentHp ? _target1 : _target2)
                : (_target1.CurrentHp <= _target2.CurrentHp ? _target1 : _target2);
        }

        if (t1Alive) return _target1;
        if (t2Alive) return _target2;
        return null;
    }

    private async Task DealExtraDamage(PlayerChoiceContext choiceContext, CardPlay cardPlay, Creature target)
    {
        for (int i = 0; i < 2; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCardCompatibility(this, cardPlay)
                .Targeting(target)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}
