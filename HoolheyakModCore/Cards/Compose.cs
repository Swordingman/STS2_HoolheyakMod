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
public class Compose : HoolheyakBaseCard, IVariableCard
{
    public Compose() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Compose-ReduceOne", 3m)
    ];

    public bool CanAllIn => true;

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay, bool isAutoTriggered = false)
    {
        return [
            new VariableChoice(async context => {
                var hand = PileType.Hand.GetPile(Owner);
                if (hand.Cards.Count == 0)
                    return;

                if (isAutoTriggered)
                {
                    var card = hand.Cards[Owner.RunState.Rng.Shuffle.NextInt(hand.Cards.Count)];
                    card.EnergyCost.AddThisTurnOrUntilPlayed(-3, true);
                }
                else
                {
                    var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
                    var selected = (await CardSelectCmd.FromHand(context, Owner, prefs, null, this)).ToList();
                    foreach (var card in selected)
                    {
                        card.EnergyCost.AddThisTurnOrUntilPlayed(-3, true);
                    }
                }
            }),
            new VariableChoice(async context => {
                var hand = PileType.Hand.GetPile(Owner);
                foreach (var card in hand.Cards.ToList())
                {
                    card.EnergyCost.AddThisTurnOrUntilPlayed(-1, true);
                }
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
        EnergyCost.UpgradeBy(-1);
    }
}
