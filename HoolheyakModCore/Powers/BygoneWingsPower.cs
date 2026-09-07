using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class BygoneWingsPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(BygoneWingsPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(BygoneWingsPower)}.png";

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player == null || cardPlay.Card.Owner?.Creature != Owner || Amount <= 0)
            return;

        var card = cardPlay.Card;

        // 非本职业牌，且不是状态/诅咒
        if (card.Type == CardType.Curse || card.Type == CardType.Status)
            return;

        if (card.Pool == ModelDb.CardPool<HoolheyakModCardPool>())
            return;

        Flash();

        var player = Owner.Player;
        var hand = PileType.Hand.GetPile(player);
        var rng = player.RunState.Rng.Shuffle;

        for (int i = 0; i < (int)Amount; i++)
        {
            var candidates = hand.Cards
                .Where(c => !ReferenceEquals(c, card)
                         && c.EnergyCost.GetWithModifiers(CostModifiers.All) > 0)
                .ToList();

            if (candidates.Count == 0)
                break;

            var target = candidates[rng.NextInt(candidates.Count)];

            target.EnergyCost.AddThisTurnOrUntilPlayed(-1, true);
        }
    }
}
