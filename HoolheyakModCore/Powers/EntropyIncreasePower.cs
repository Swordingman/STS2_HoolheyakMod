using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class EntropyIncreasePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(EntropyIncreasePower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(EntropyIncreasePower)}.png";

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner != player.Creature || Amount <= 0)
            return;

        Flash();

        var candidates = ModelDb.AllCards
            .Where(c => c.Type == CardType.Power && c.CanBeGeneratedInCombat)
            .ToList();

        if (candidates.Count == 0)
            return;

        var generatedCards = CardFactory.GetDistinctForCombat(
            player,
            candidates,
            (int)Amount,
            player.RunState.Rng.CombatCardGeneration
        ).ToList();

        foreach (var randomPower in generatedCards)
        {
            await CardPileCmd.AddGeneratedCardToCombat(randomPower, PileType.Hand, player);
        }
    }
}
