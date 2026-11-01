using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Scripts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class ComorbidityModel : HoolheyakBaseCard
{
    public ComorbidityModel() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true) { }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [HoolheyakKeywords.Slough];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(13, ValueProp.Move), new DynamicVar("ComorbidityModel-Bonus", 2m)];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Target == null) return;
        int extra = SloughMechanics.Debuffs(Owner.Creature).Count * DynamicVars["ComorbidityModel-Bonus"].IntValue;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + extra)
            .FromCardCompatibility(this, play)
            .WithHitFx(VfxCmd.bloodyImpactPath).Targeting(play.Target).Execute(context);
    }

    protected override void OnUpgrade() => DynamicVars["ComorbidityModel-Bonus"].UpgradeValueBy(1);
}
