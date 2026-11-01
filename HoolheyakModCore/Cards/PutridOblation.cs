using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Code.Variables;
using HoolheyakMod.Scripts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class PutridOblation : HoolheyakBaseCard, IVariableCard
{
    public PutridOblation() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true) { }
    public override bool GainsBlock => true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(15, ValueProp.Move), new DynamicVar("PutridOblation-Draw", 3m),
         new DynamicVar("PutridOblation-Energy", 3m)];

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext context, CardPlay play, bool isAutoTriggered = false) =>
    [
        new VariableChoice(async c =>
        {
#if STS2_BETA
            await CreatureCmd.LoseBlock(c, Owner.Creature, Owner.Creature.Block, Owner.Creature);
#else
            await CreatureCmd.LoseBlock(Owner.Creature, Owner.Creature.Block);
#endif
            await PowerCmd.Apply<OblationNoBlockPower>(c, Owner.Creature, 1, Owner.Creature, this);
            await CardPileCmd.Draw(c, DynamicVars["PutridOblation-Draw"].IntValue, Owner);
            await PlayerCmd.GainEnergy(DynamicVars["PutridOblation-Energy"].IntValue, Owner);
        }),
        new VariableChoice(async c =>
        {
            await PowerCmd.Apply<OblationNoDrawPower>(c, Owner.Creature, 1, Owner.Creature, this);
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, play);
            await PlayerCmd.GainEnergy(DynamicVars["PutridOblation-Energy"].IntValue, Owner);
        }),
        new VariableChoice(async c =>
        {
            await PowerCmd.Apply<OblationNoEnergyPower>(c, Owner.Creature, 1, Owner.Creature, this);
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, play);
            await CardPileCmd.Draw(c, DynamicVars["PutridOblation-Draw"].IntValue, Owner);
        })
    ];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        await PlayCastAnimation();
        await VariableCmd.Choose(context, this, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(5);
        DynamicVars["PutridOblation-Draw"].UpgradeValueBy(1);
        DynamicVars["PutridOblation-Energy"].UpgradeValueBy(1);
    }
}
