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
public class InfiltrativeNecrosis : HoolheyakBaseCard
{
    public InfiltrativeNecrosis() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true) { }
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(1, ValueProp.Move), new DynamicVar("InfiltrativeNecrosis-Hits", 12m)];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Target == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, play)
            .WithHitFx(VfxCmd.scratchPath).Targeting(play.Target)
            .WithHitCount(DynamicVars["InfiltrativeNecrosis-Hits"].IntValue).Execute(context);

        await PowerCmd.Apply<DarkShacklesPower>(context, Owner.Creature, 99, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["InfiltrativeNecrosis-Hits"].UpgradeValueBy(2);
}
