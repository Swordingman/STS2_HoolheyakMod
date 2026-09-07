using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Code.Variables;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class ApoptosisExperiment : HoolheyakBaseCard, IVariableCard
{
    public ApoptosisExperiment() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move),
        new DynamicVar("Apoptosis-Lift", 4m),
        new DynamicVar("Apoptosis-Deconstruction", 15m)
    ];

    public bool CanAllIn => true;

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay, bool isAutoTriggered = false)
    {
        var target = cardPlay.Target;
        if (target == null)
            return [];

        return [
            new VariableChoice(async context => {
                await PowerCmd.Apply<LiftPower>(
                    context,
                    target,
                    DynamicVars["Apoptosis-Lift"].IntValue,
                    Owner.Creature,
                    this
                );
            }),
            new VariableChoice(async context => {
                await PowerCmd.Apply<DeconstructionPower>(
                    context,
                    target,
                    DynamicVars["Apoptosis-Deconstruction"].IntValue,
                    Owner.Creature,
                    this
                );
            })
        ];
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompatibility(this, cardPlay)
            .Targeting(target)
            .Execute(choiceContext);

        await VariableCmd.Choose(choiceContext, this, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["Apoptosis-Lift"].UpgradeValueBy(1);
        DynamicVars["Apoptosis-Deconstruction"].UpgradeValueBy(2);
    }
}
