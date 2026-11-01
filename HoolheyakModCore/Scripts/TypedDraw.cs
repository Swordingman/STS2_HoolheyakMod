using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace HoolheyakMod.Scripts.Utils;

internal static class TypedDraw
{
    // Use native draws so restrictions, hand limits, history and draw hooks still apply.
    public static async Task Execute(PlayerChoiceContext context, Player player, CardType type, int count)
    {
        var combat = player.Creature.CombatState;
        if (combat == null) return;
        var draw = PileType.Draw.GetPile(player);
        var discard = PileType.Discard.GetPile(player);
        for (int i = 0; i < count; i++)
        {
            if (PileType.Hand.GetPile(player).Cards.Count >= CardPile.MaxCardsInHand ||
                !Hook.ShouldDraw(combat, player, false, out _)) return;
            var card = draw.Cards.FirstOrDefault(c => c.Type == type);
            if (card == null && discard.Cards.Any(c => c.Type == type))
            {
                await CardPileCmd.Shuffle(context, player);
                card = draw.Cards.FirstOrDefault(c => c.Type == type);
            }
            if (card == null) return;
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top);
            var drawn = await CardPileCmd.Draw(context, 1, player);
            if (!drawn.Any()) return;
        }
    }
}
