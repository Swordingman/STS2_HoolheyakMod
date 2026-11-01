using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using HoolheyakMod.Scripts.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HoolheyakMod.Code.Powers;

public class ReproducibilityCrisisPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(ReproducibilityCrisisPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(ReproducibilityCrisisPower)}.png";

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay play)
    {
        // Count each completed series once, using the count fixed before play.
        // Conditional replay may already have changed by the time it resolves.
        if (Owner?.Player is { PlayerCombatState: { } combat } player &&
            play.Card.Owner == player && play.IsLastInSeries && play.PlayCount > 1 && Amount > 0)
        {
            Flash();
            // Match model IDs, so upgraded copies belong to the same card family.
            // Include the played card even if its effect already moved it out of a pile.
            var matchingCards = combat.AllPiles.SelectMany(pile => pile.Cards)
                .Append(play.Card)
                .Where(card => card.Owner == player && card.Id == play.Card.Id)
                .Distinct<CardModel>(ReferenceEqualityComparer.Instance)
                .ToArray();
            foreach (var card in matchingCards)
                card.BaseReplayCount += Amount;
        }
        return Task.CompletedTask;
    }
}
