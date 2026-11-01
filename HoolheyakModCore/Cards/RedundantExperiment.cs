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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class RedundantExperiment : HoolheyakBaseCard, IVariableCard
{
    private List<CardModel> _selectedCards = [];

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _selectedCards = new List<CardModel>(_selectedCards);
    }

    public RedundantExperiment() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Redundant-Count", 1m)
    ];

    public bool CanAllIn => true;

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay, bool isAutoTriggered = false)
    {
        if (isAutoTriggered)
        {
            return [
                new VariableChoice(async context => {
                    var hand = PileType.Hand.GetPile(Owner);
                    var pool = hand.Cards.ToList();
                    pool.RemoveAll(c => ReferenceEquals(c, this));

                    int count = Math.Min(DynamicVars["Redundant-Count"].IntValue, pool.Count);
                    var picks = new List<CardModel>();

                    while (picks.Count < count && pool.Count > 0)
                    {
                        int index = Owner.RunState.Rng.Shuffle.NextInt(pool.Count);
                        picks.Add(pool[index]);
                        pool.RemoveAt(index);
                    }

                    if (picks.Count == 0)
                        return;

                    bool usePower = Owner.RunState.Rng.Shuffle.NextInt(2) == 0;
                    if (usePower)
                    {
                        await RedundantExperimentPower.ApplyToCards(context, Owner, picks, this);
                    }
                    else
                    {
                        foreach (var card in picks)
                        {
                            await CardCmd.Discard(context, card);
                        }

                        if (picks.Count > 0)
                        {
                            await CardPileCmd.Draw(context, picks.Count, Owner);
                        }
                    }
                })
            ];
        }

        return [
            new VariableChoice(async context => {
                await RedundantExperimentPower.ApplyToCards(context, Owner, _selectedCards, this);
            }),
            new VariableChoice(async context => {
                foreach (var card in _selectedCards)
                {
                    await CardCmd.Discard(context, card);
                }

                if (_selectedCards.Count > 0)
                {
                    await CardPileCmd.Draw(context, _selectedCards.Count, Owner);
                }
            })
        ];
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayCastAnimation();

        var player = Owner;
        if (player == null || player.Creature == null)
            return;

        var hand = PileType.Hand.GetPile(player);
        int maxCount = Math.Min(DynamicVars["Redundant-Count"].IntValue, hand.Cards.Count);
        if (maxCount <= 0)
            return;

        var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, maxCount);
        var selected = (await CardSelectCmd.FromHand(choiceContext, player, prefs, null, this)).ToList();
        if (selected.Count == 0)
            return;

        _selectedCards.Clear();
        _selectedCards.AddRange(selected);

        await VariableCmd.Choose(choiceContext, this, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Redundant-Count"].UpgradeValueBy(1);
    }
}
