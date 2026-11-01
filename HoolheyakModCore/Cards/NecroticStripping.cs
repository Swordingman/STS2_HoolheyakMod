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
public class NecroticStripping : HoolheyakBaseCard
{
    public NecroticStripping() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true) { }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [HoolheyakKeywords.Slough];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(5, ValueProp.Move), new DynamicVar("NecroticStripping-Hits", 3m)];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Target == null) return;
        bool slough = SloughMechanics.Active(Owner.Creature);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, play)
            .WithHitFx(VfxCmd.bloodyImpactPath).Targeting(play.Target)
            .WithHitCount(DynamicVars["NecroticStripping-Hits"].IntValue).Execute(context);

        if (!slough) return;
        var debuffs = SloughMechanics.Debuffs(Owner.Creature);
        foreach (var debuff in debuffs)
        {
            if (await SloughMechanics.RemoveDebuff(context, debuff, this))
                await TriggerErudition.Trigger(context, Owner.Creature);
        }
    }

    protected override void OnUpgrade() => DynamicVars["NecroticStripping-Hits"].UpgradeValueBy(1);
}
