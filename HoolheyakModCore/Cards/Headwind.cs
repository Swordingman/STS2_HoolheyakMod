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
    private bool _attacking;
    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _attacking = false;
    }
    public Headwind() : base(2, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4, ValueProp.Move)
    ];

    public override int CanonicalReplayCount => 1;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null || Owner == null) return;

        _attacking = true;
        try
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCardCompatibility(this, cardPlay)
                .WithHitFx(VfxCmd.flyingSlashPath)
                .TargetingRandomOpponents(CombatState)
                .Execute(choiceContext);
        }
        finally { _attacking = false; }
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext context, Creature? dealer,
        DamageResult result, ValueProp props, Creature target, CardModel? source)
    {
        if (_attacking && ReferenceEquals(source, this) && dealer == Owner.Creature && target.IsAlive)
            await PowerCmd.Apply<LiftPower>(context, target, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 伤害 4 -> 5
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}
