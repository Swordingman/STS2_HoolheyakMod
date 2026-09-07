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
public class ForbiddenKnowledge : HoolheyakBaseCard, IVariableCard
{
    public ForbiddenKnowledge() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Forbidden-Draw", 1m),
        new DynamicVar("Forbidden-Aoe", 5m)
    ];

    public bool CanAllIn => true;

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay, bool isAutoTriggered = false)
    {
        return [
            new VariableChoice(async context => {
                await PowerCmd.Apply<ForbiddenKnowledgeEruPower>(
                    context,
                    Owner.Creature,
                    DynamicVars["Forbidden-Draw"].IntValue,
                    Owner.Creature,
                    this
                );
            }),
            new VariableChoice(async context => {
                await PowerCmd.Apply<ForbiddenKnowledgeMeaPower>(
                    context,
                    Owner.Creature,
                    DynamicVars["Forbidden-Aoe"].IntValue,
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
        AddKeyword(CardKeyword.Innate);
    }
}
