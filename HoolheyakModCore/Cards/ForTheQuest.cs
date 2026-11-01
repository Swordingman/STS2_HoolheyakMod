using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class ForTheQuest : HoolheyakBaseCard
{
    public ForTheQuest() : base(0, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy, true) { }
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move)];

    internal static int GetLiftAmount(Creature target) =>
        (target.GetPower<GravityPower>()?.Amount ?? GravityPower.CalculateGravity(target)) / 2;

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Target == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, play)
            .WithHitFx(VfxCmd.starryImpactVfx).Targeting(play.Target).Execute(context);
        if (play.Target.IsDead) return;
        int lift = GetLiftAmount(play.Target);
        if (lift > 0)
            await PowerCmd.Apply<LiftPower>(context, play.Target, lift, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}
