using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Code.Variables;
using MegaCrit.Sts2.Core.CardSelection;
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
public class Linger : HoolheyakBaseCard, IVariableCard
{
    private readonly List<CardModel> _selectedCards = [];

    public Linger() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self, true)
    {
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(6, ValueProp.Move),
        new DynamicVar("Linger-Look", 1m),
        new DynamicVar("Linger-Draw", 1m)
    ];

    public bool CanBeAutoTriggered => false;

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay, bool isAutoTriggered = false)
    {
        if (_selectedCards.Count == 0)
            return [];

        return [
            new VariableChoice(async context => {
                foreach (var card in _selectedCards)
                {
                    await CardPileCmd.Add(card, PileType.Discard);
                }
            }),
            new VariableChoice(async context => {
                foreach (var card in _selectedCards)
                {
                    await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Bottom);
                }
            })
        ];
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);

        await CardPileCmd.ShuffleIfNecessary(choiceContext, Owner);

        var drawPile = PileType.Draw.GetPile(Owner);
        var lookAmount = DynamicVars["Linger-Look"].IntValue;
        var topCards = drawPile.Cards.Take(lookAmount).ToList();

        if (topCards.Count > 0)
        {
            var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 0, topCards.Count);
            var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, topCards, Owner, prefs)).ToList();

            if (selected.Count > 0)
            {
                _selectedCards.Clear();
                _selectedCards.AddRange(selected);

                await VariableCmd.Choose(choiceContext, this, cardPlay);
            }
        }

        await CardPileCmd.Draw(choiceContext, DynamicVars["Linger-Draw"].IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Linger-Look"].UpgradeValueBy(1);
        DynamicVars["Linger-Draw"].UpgradeValueBy(1);
    }
}
