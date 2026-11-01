using BaseLib.Abstracts;
using HoolheyakMod.Code.Relics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Events;

public sealed class IntersectionEvent : CustomEventModel
{
    public override string? CustomInitialPortraitPath => "res://HoolheyakMod/images/events/tin_man.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new GoldVar(50),
        new DynamicVar("HpLoss", 5)
    ];

    public override void CalculateVars()
    {
        base.CalculateVars();
        DynamicVars["HpLoss"].BaseValue = Owner?.RunState.AscensionLevel >= (int)MegaCrit.Sts2.Core.Entities.Ascension.AscensionLevel.DeadlyEnemies ? 8 : 5;
    }

    public override bool IsAllowed(IRunState runState) =>
        runState.CurrentActIndex is 0 or 1 &&
        runState.Players.Count == 1 &&
        runState.Players[0].Character is HoolheyakMod.Code.Character.HoolheyakMod;

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var options = new List<EventOption>();

        options.Add(new EventOption(this, Owner?.Deck.Cards.Any(c => c.IsUpgradable) == true ? PoliteReply : null,
            new LocString("events", $"{Id.Entry}.pages.INITIAL.options.POLITE_REPLY.title"),
            new LocString("events", $"{Id.Entry}.pages.INITIAL.options.POLITE_REPLY.description"),
            "POLITE_REPLY", Array.Empty<IHoverTip>()));

        options.Add(new EventOption(this, Quarrel,
            new LocString("events", $"{Id.Entry}.pages.INITIAL.options.QUARREL.title"),
            new LocString("events", $"{Id.Entry}.pages.INITIAL.options.QUARREL.description"),
            "QUARREL", Array.Empty<IHoverTip>()));

        options.Add(new EventOption(this, SeekHelp,
            new LocString("events", $"{Id.Entry}.pages.INITIAL.options.SEEK_HELP.title"),
            new LocString("events", $"{Id.Entry}.pages.INITIAL.options.SEEK_HELP.description"),
            "SEEK_HELP", Array.Empty<IHoverTip>()));

        return options;
    }

    private async Task PoliteReply()
    {
        if (Owner == null) return;

        await PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, Owner);

        var upgradable = Owner.Deck.Cards.Where(c => c.IsUpgradable).ToList();
        if (upgradable.Count > 0)
        {
            var card = Owner.RunState.Rng.UpFront.NextItem(upgradable);
            if (card != null)
                CardCmd.Upgrade(card);
        }

        SetEventFinished(PageDescription("POLITE_REPLY_DONE"));
    }

    private async Task Quarrel()
    {
        if (Owner == null) return;

        #if STS2_BETA
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            DynamicVars["HpLoss"].IntValue,
            MegaCrit.Sts2.Core.ValueProps.ValueProp.Unblockable,
            (MegaCrit.Sts2.Core.Entities.Creatures.Creature?)null,
            null,
            null);
        #else
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            DynamicVars["HpLoss"].IntValue,
            MegaCrit.Sts2.Core.ValueProps.ValueProp.Unblockable,
            (MegaCrit.Sts2.Core.Entities.Creatures.Creature?)null,
            null);
        #endif

        var removable = Owner.Deck.Cards.Where(c => c.IsRemovable).ToList();
        if (Owner.Creature.IsAlive && removable.Count > 0)
        {
            var cardsToRemove = (await CardSelectCmd.FromDeckForRemoval(
                Owner,
                new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, Math.Min(2, removable.Count))
            )).ToList();

            if (cardsToRemove.Count > 0)
            {
                await CardPileCmd.RemoveFromDeck(cardsToRemove);
            }
        }

        SetEventFinished(PageDescription("QUARREL_DONE"));
    }

    private async Task SeekHelp()
    {
        if (Owner == null) return;

        await RewardsCmd.OfferCustom(Owner, [
            new PotionReward(Owner),
            new PotionReward(Owner)
        ]);

        SetEventFinished(PageDescription("SEEK_HELP_DONE"));
    }
}

