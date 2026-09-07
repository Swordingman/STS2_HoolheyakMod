using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Relics;
using HoolheyakMod.Code.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class RecursiveExperiment : HoolheyakBaseCard, IVariableCard
{
    public RecursiveExperiment() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public bool CanBeAutoTriggered => false;

    public IReadOnlyList<VariableChoice> GetVariableChoices(PlayerChoiceContext choiceContext, CardPlay cardPlay, bool isAutoTriggered = false)
    {
        return [
            new VariableChoice(async context => {
                // 终止本次实验。
            }),
            new VariableChoice(async context => {
                var player = Owner;
                if (player == null || player.Creature == null)
                    return;

                if (player.PlayerCombatState == null || (int)player.PlayerCombatState.Energy < 1)
                    return;

                await PlayerCmd.LoseEnergy(1, player);

                var candidates = new List<CardModel>();
                candidates.AddRange(PileType.Draw.GetPile(player).Cards);
                candidates.AddRange(PileType.Discard.GetPile(player).Cards);
                candidates.AddRange(PileType.Hand.GetPile(player).Cards);
                candidates.RemoveAll(c => object.ReferenceEquals(c, this));

                if (candidates.Count == 0)
                    return;

                var picked = player.RunState.Rng.CombatCardSelection.NextItem(candidates);
                if (picked == null)
                    return;

                var removedType = picked.Type;
                var clone = picked.CreateClone();

                await CardPileCmd.RemoveFromCombat(picked, false);

                var deckCard = player.Deck.Cards.FirstOrDefault(c => c.Id == picked.Id);
                if (deckCard != null)
                {
                    await CardPileCmd.RemoveFromDeck(deckCard);
                }

                await CardCmd.AutoPlay(context, clone, null);

                if (removedType == CardType.Curse || removedType == CardType.Status)
                {
                    player.GetRelic<FailedExperimentProduct>()?.IncrementCounter();
                }
                else
                {
                    var options = CardCreationOptions.ForNonCombatWithDefaultOdds(
                        new[] { player.Character.CardPool },
                        card => card.Type == removedType
                    );
                    var cardReward = new CardReward(options, 3, player);
                    await RewardsCmd.OfferCustom(player, new List<Reward> { cardReward });
                }

                if (player.PlayerCombatState != null && (int)player.PlayerCombatState.Energy >= 1)
                {
                    await VariableCmd.Choose(context, this, cardPlay);
                }
            })
        ];
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        if (player == null || player.Creature == null)
            return;

        if (player.PlayerCombatState == null || (int)player.PlayerCombatState.Energy < 1)
            return;

        await VariableCmd.Choose(choiceContext, this, cardPlay);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
