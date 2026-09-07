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
public class Foreshadowing : HoolheyakBaseCard
{
    public Foreshadowing() : base(2, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(10, ValueProp.Move),
        new DynamicVar("Foreshadowing-Magic", 2m),
        new DynamicVar("Foreshadowing-Meander", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 10 点格挡
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);

        // 获得 2 层博览和 1 层逶迤
        await PowerCmd.Apply<EruditionPower>(choiceContext, Owner.Creature, DynamicVars["Foreshadowing-Magic"].IntValue, Owner.Creature, this);
        await PowerCmd.Apply<MeanderPower>(choiceContext, Owner.Creature, DynamicVars["Foreshadowing-Meander"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 格挡 10 -> 12，博览 2 -> 3，逶迤 1 -> 2
        DynamicVars.Block.UpgradeValueBy(2);
        DynamicVars["Foreshadowing-Magic"].UpgradeValueBy(1);
        DynamicVars["Foreshadowing-Meander"].UpgradeValueBy(1);
    }
}
