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
public class Coercion : HoolheyakBaseCard
{
    public Coercion() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move),
        new DynamicVar("Coercion-Energy", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;

        // 造成 12 点伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 近似：若目标有浮空，返还能量并获得少量格挡（原版为坠落伤害转化的格挡）
        if (cardPlay.Target.GetPower<LevitatePower>() != null)
        {
            await PlayerCmd.GainEnergy(DynamicVars["Coercion-Energy"].IntValue, Owner);
            await CreatureCmd.GainBlock(Owner.Creature, 5, ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害 12 -> 16，返还能量 1 -> 2
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars["Coercion-Energy"].UpgradeValueBy(1);
    }
}
