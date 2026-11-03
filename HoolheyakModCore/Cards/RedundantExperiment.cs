using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Variables;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class RedundantExperiment : HoolheyakBaseCard, IVariableCard
{
    private List<CardModel> _selectedCards = [];

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _selectedCards = [];
    }

    public RedundantExperiment() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None, true) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Redundant-Count", 2m)];
    public bool CanAllIn => true;

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext context,
        CardPlay play, bool isAutoTriggered = false)
    {
        if (isAutoTriggered)
            return [new VariableChoice(async choiceContext =>
            {
                // Automatic variable triggers never open a hand-selection screen.
                var pool = PileType.Hand.GetPile(Owner).Cards.Where(card => card != this).ToList();
                var selected = new List<CardModel>();
                int count = Math.Min(DynamicVars["Redundant-Count"].IntValue, pool.Count);
                while (selected.Count < count)
                {
                    int index = Owner.RunState.Rng.Shuffle.NextInt(pool.Count);
                    selected.Add(pool[index]);
                    pool.RemoveAt(index);
                }
                bool putOnTop = Owner.RunState.Rng.Shuffle.NextInt(2) == 1;
                await Resolve(choiceContext, selected, putOnTop);
            })];

        var cards = _selectedCards.ToArray();
        return [
            new VariableChoice(choiceContext => Resolve(choiceContext, cards, false)),
            new VariableChoice(choiceContext => Resolve(choiceContext, cards, true))
        ];
    }

    private async Task Resolve(PlayerChoiceContext context, IReadOnlyList<CardModel> cards, bool putOnTop)
    {
        // Use the native shuffle command so discard recycling, RNG and shuffle
        // hooks all resolve before the selected cards change piles.
        await CardPileCmd.Shuffle(context, Owner);
        if (putOnTop)
        {
            // First selected card stays at the top. Draw/Discard also allow the
            // second branch of All-In to handle cards moved by its first branch.
            foreach (var card in cards.Reverse())
                if (card.Owner == Owner && card.Pile?.Type is PileType.Hand or PileType.Draw or PileType.Discard)
                    await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top);
        }
        else
        {
            foreach (var card in cards)
                if (card.Owner == Owner && card.Pile?.Type == PileType.Hand)
                    await CardCmd.Discard(context, card);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        _selectedCards.Clear();
        await PlayCastAnimation();
        var hand = PileType.Hand.GetPile(Owner);
        bool CanSelect(CardModel card) => card != this;
        int count = Math.Min(DynamicVars["Redundant-Count"].IntValue, hand.Cards.Count(CanSelect));
        if (count > 0)
        {
            var prompt = SelectionScreenPrompt;
            DynamicVars.AddTo(prompt);
            var prefs = new CardSelectorPrefs(prompt, 0, count);
            _selectedCards.AddRange((await CardSelectCmd.FromHand(context, Owner, prefs, CanSelect, this))
                .Where(card => card.Owner == Owner && card.Pile?.Type == PileType.Hand && CanSelect(card)));
        }
        // Selecting zero cards still performs the chosen shuffle.
        await VariableCmd.Choose(context, this, play);
    }

    protected override void OnUpgrade() => DynamicVars["Redundant-Count"].UpgradeValueBy(2);
}
