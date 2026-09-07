using HoolheyakMod.Code.Variables;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Utils;

public static class StargazingLogic
{
    public static async Task Execute<T>(PlayerChoiceContext choiceContext, CardPlay cardPlay, T source,
        LocString selectionPrompt, int amount, List<CardModel> selectedCards)
        where T : CardModel, IVariableCard
    {
        selectedCards.Clear();

        if (amount <= 0)
            return;

        var player = source.Owner;

        await CardPileCmd.ShuffleIfNecessary(choiceContext, player);

        var topCards = PileType.Draw.GetPile(player).Cards.Take(amount).ToList();
        if (topCards.Count == 0)
            return;

        var prefs = new CardSelectorPrefs(selectionPrompt, 0, topCards.Count);
        selectedCards.AddRange(await CardSelectCmd.FromSimpleGrid(choiceContext, topCards, player, prefs));

        if (selectedCards.Count == 0)
            return;

        await VariableCmd.Choose(choiceContext, source, cardPlay);
    }

    public static IReadOnlyList<VariableChoice> GetVariableChoices(IReadOnlyList<CardModel> selectedCards)
    {
        if (selectedCards.Count == 0)
            return [];

        return [
            new VariableChoice(async context =>
            {
                foreach (var card in selectedCards)
                    await CardPileCmd.Add(card, PileType.Discard);
            }),
            new VariableChoice(async context =>
            {
                foreach (var card in selectedCards)
                    await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Bottom);
            })
        ];
    }
}