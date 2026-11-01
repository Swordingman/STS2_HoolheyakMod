using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Runs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(SharedRelicPool))]
public class AstronomicalTelescope : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(AstronomicalTelescope)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(AstronomicalTelescope)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(AstronomicalTelescope)}.png";

    public override async Task AfterObtained()
    {
        await base.AfterObtained();

        if (Owner == null) return;

        var starters = Owner.Deck.Cards
            .Where(IsStarter)
            .ToList();

        var colorless = ModelDb.CardPool<ColorlessCardPool>();
        if (starters.Count > 0)
        {
            var options = CardCreationOptions.ForNonCombatWithUniformOdds([colorless],
                card => card.Type is CardType.Attack or CardType.Skill or CardType.Power &&
                    card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare)
                .GetPossibleCards(Owner).ToArray();
            if (options.Length == 0) return;

            var prefs = new CardSelectorPrefs(
                new LocString("relics", $"{Id.Entry}.selectionScreenPrompt"), 1)
            {
                RequireManualConfirmation = true,
                Cancelable = false
            };
            var original = await SelectStarterForTransformation(starters, options, prefs);
            if (original == null) return;

            Flash();
            // AromaOfChaos uses this preview style after its transform selection.
            await CardCmd.Transform([new CardTransformation(original, options)], Owner.RunState.Rng.UpFront,
                CardPreviewStyle.EventLayout);
            return;
        }

        var rareOptions = CardCreationOptions.ForNonCombatWithDefaultOdds([colorless],
            card => card.Rarity == CardRarity.Rare &&
                card.Type is CardType.Attack or CardType.Skill or CardType.Power)
            .WithRngOverride(Owner.RunState.Rng.UpFront);
        var reward = CardFactory.CreateForReward(Owner, 1, rareOptions).FirstOrDefault();
        if (reward == null) return;
        Flash();
        await CardPileCmd.Add(reward.Card, PileType.Deck);
    }

    private async Task<CardModel?> SelectStarterForTransformation(
        IReadOnlyList<CardModel> starters, CardModel[] options, CardSelectorPrefs prefs)
    {
        IEnumerable<CardModel> selected;
        if (CardSelectCmd.Selector is { } selector)
        {
            selected = await selector.GetSelectedCards(starters, prefs.MinSelect, prefs.MaxSelect);
        }
        else
        {
            // Follow FromDeckForTransformation's synchronization and dedicated UI,
            // supplying only eligible starters instead of the entire deck.
            var run = RunManager.Instance;
            var synchronizer = run.PlayerChoiceSynchronizer;
            var choiceId = synchronizer.ReserveChoiceId(Owner);
            if (LocalContext.IsMe(Owner) && run.NetService.Type != NetGameType.Replay)
            {
#if STS2_BETA
                if (CardSelectCmd.LocalSelector is { } localSelector)
                {
                    selected = await localSelector.GetSelectedCards(starters, prefs.MinSelect, prefs.MaxSelect);
                }
                else
#endif
                {
                    var screen = NDeckTransformSelectScreen.ShowScreen(
                        starters, card => new CardTransformation(card, options), prefs);
                    selected = await screen.CardsSelected();
                }
                selected = selected.ToList();
                synchronizer.SyncLocalChoice(Owner, choiceId, PlayerChoiceResult.FromMutableDeckCards(selected));
            }
            else
            {
                selected = (await synchronizer.WaitForRemoteChoice(Owner, choiceId)).AsDeckCards();
            }
        }
        return selected.FirstOrDefault(card => starters.Contains(card) && IsStarter(card));
    }

    internal static bool IsStarter(CardModel card) => card.Rarity == CardRarity.Basic && card.IsTransformable &&
        (card.Tags.Contains(CardTag.Strike) || card.Tags.Contains(CardTag.Defend));
}
