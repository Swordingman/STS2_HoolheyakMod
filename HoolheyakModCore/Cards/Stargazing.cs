using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Variables;
using HoolheyakMod.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class Stargazing : HoolheyakBaseCard, IVariableCard
{
    private readonly List<CardModel> _selectedCards = [];

    public Stargazing() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Stargazing-Magic", 3m)
    ];

    public bool CanBeAutoTriggered => false;

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay,
        bool isAutoTriggered = false)
    {
        return StargazingLogic.GetVariableChoices(_selectedCards);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var amount = DynamicVars["Stargazing-Magic"].IntValue;
        await StargazingLogic.Execute(choiceContext, cardPlay, this, SelectionScreenPrompt, amount, _selectedCards);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Stargazing-Magic"].UpgradeValueBy(2);
    }
}