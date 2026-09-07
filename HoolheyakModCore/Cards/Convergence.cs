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
public class Convergence : HoolheyakBaseCard
{
    public Convergence() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(8, ValueProp.Move),
        new DynamicVar("Convergence-Cap", 15m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 8 点基础格挡
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);

        // 统计所有敌人身上的重力层数，并以此获得额外格挡（上限 magicNumber）
        if (CombatState != null)
        {
            int totalGravity = CombatState.Enemies
                .Where(e => e.IsAlive)
                .Sum(e => e.GetPower<GravityPower>()?.Amount ?? 0);

            int cap = DynamicVars["Convergence-Cap"].IntValue;
            int extraBlock = System.Math.Min(totalGravity, cap);
            if (extraBlock > 0)
                await CreatureCmd.GainBlock(Owner.Creature, extraBlock, ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        // 格挡 8 -> 10，上限 15 -> 25
        DynamicVars.Block.UpgradeValueBy(2);
        DynamicVars["Convergence-Cap"].UpgradeValueBy(10);
    }
}
