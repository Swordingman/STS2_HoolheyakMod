using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Code.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class AncientCovenant : HoolheyakBaseCard, IVariableCard
{
    public AncientCovenant() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Covenant-Magic", 1m)
    ];

    public bool CanAllIn => true;

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay, bool isAutoTriggered = false)
    {
        return [
            new VariableChoice(async context => {
                await PowerCmd.Apply<CovenantStrengthPower>(
                    context,
                    Owner.Creature,
                    DynamicVars["Covenant-Magic"].IntValue,
                    Owner.Creature,
                    this
                );
            }),
            new VariableChoice(async context => {
                await PowerCmd.Apply<CovenantDexterityPower>(
                    context,
                    Owner.Creature,
                    DynamicVars["Covenant-Magic"].IntValue,
                    Owner.Creature,
                    this
                );
            })
        ];
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || Owner.Creature == null)
            return;

        await VariableCmd.Choose(choiceContext, this, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Covenant-Magic"].UpgradeValueBy(1);
    }
}
