using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Cards;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class ForgetfulnessPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => "res://HoolheyakMod/images/powers/ForgetfulnessPower.png";
    public override string? CustomBigIconPath => "res://HoolheyakMod/images/powers/large/ForgetfulnessPower.png";

    internal static void PrepareGeneratedCard(CardModel card)
    {
        card.SetToFreeThisTurn();
        // BaseLib 的移出机制在整组重放结算后处理，也适用于能力牌及自动打出。
        card.AddKeyword(BaseLibKeywords.Purge);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (Owner?.Player != player || Amount <= 0) return;
        var candidates = ModelDb.AllCards.Where(card =>
            card.CanBeGeneratedInCombat &&
            card.Type is CardType.Attack or CardType.Skill or CardType.Power &&
            card.Rarity != CardRarity.Token).ToList();
        if (candidates.Count == 0) return;
        int count = Amount;
        for (int i = 0; i < count; i++)
        {
            if (!Owner.Powers.Contains(this)) break;
            var choices = CardFactory.GetDistinctForCombat(player, candidates, 3,
                player.RunState.Rng.CombatCardGeneration).ToList();
            if (choices.Count == 0) return;
            Flash();
            var prefs = new CardSelectorPrefs(new LocString("powers",
                "HOOLHEYAKMOD-FORGETFULNESS_POWER.select_message"), 1);
            var chosen = (await CardSelectCmd.FromSimpleGrid(context, choices, player, prefs)).FirstOrDefault();
            if (chosen == null) continue;
            PrepareGeneratedCard(chosen);
            var destination = PileType.Hand.GetPile(player).Cards.Count < CardPile.MaxCardsInHand
                ? PileType.Hand : PileType.Discard;
            await CardPileCmd.AddGeneratedCardToCombat(chosen, destination, player);
        }
    }
}
