using BaseLib.Abstracts;
using HoolheyakMod.Code.Relics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Events;

public sealed class FalseDomeEvent : CustomEventModel
{
    public override string? CustomInitialPortraitPath => "res://HoolheyakMod/images/events/false_dome.png";

    public override bool IsAllowed(IRunState runState) => true;

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var options = new List<EventOption>();

        options.Add(new EventOption(this, RememberPioneer,
            new LocString("events", "HOOLHEYAKMOD-FALSE_DOME.pages.INITIAL.options.REMEMBER_PIONEER.title"),
            new LocString("events", "HOOLHEYAKMOD-FALSE_DOME.pages.INITIAL.options.REMEMBER_PIONEER.description"),
            "REMEMBER_PIONEER", HoverTipFactory.FromRelic<StarryRevelation>()));

        bool canUpgrade = Owner != null && Owner.Deck.Cards.Count(c => c.IsUpgradable) >= 3;
        options.Add(new EventOption(this, canUpgrade ? AnalyzeTrajectory : null,
            new LocString("events", "HOOLHEYAKMOD-FALSE_DOME.pages.INITIAL.options.ANALYZE_TRAJECTORY.title"),
            new LocString("events", "HOOLHEYAKMOD-FALSE_DOME.pages.INITIAL.options.ANALYZE_TRAJECTORY.description"),
            "ANALYZE_TRAJECTORY", Array.Empty<IHoverTip>()));

        bool hasCurses = Owner != null && Owner.Deck.Cards.Any(c => c.Type == CardType.Curse);
        options.Add(new EventOption(this, hasCurses ? ChaseFuture : null,
            new LocString("events", "HOOLHEYAKMOD-FALSE_DOME.pages.INITIAL.options.CHASE_FUTURE.title"),
            new LocString("events", "HOOLHEYAKMOD-FALSE_DOME.pages.INITIAL.options.CHASE_FUTURE.description"),
            "CHASE_FUTURE", Array.Empty<IHoverTip>()));

        return options;
    }

    private async Task RememberPioneer()
    {
        if (Owner == null) return;

        await RelicCmd.Obtain(ModelDb.Relic<StarryRevelation>().ToMutable(), Owner);
        SetEventFinished(PageDescription("REMEMBERED_PIONEER"));
    }

    private async Task AnalyzeTrajectory()
    {
        if (Owner == null) return;

        var cards = (await CardSelectCmd.FromDeckForUpgrade(
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 3)
        )).ToList();

        foreach (var card in cards)
        {
            CardCmd.Upgrade(card);
        }

        SetEventFinished(PageDescription("ANALYZED_TRAJECTORY"));
    }

    private async Task ChaseFuture()
    {
        if (Owner == null) return;

        var curses = Owner.Deck.Cards.Where(c => c.Type == CardType.Curse).ToList();
        if (curses.Count > 0)
        {
            await CardPileCmd.RemoveFromDeck(curses);
        }

        SetEventFinished(PageDescription("CHASED_FUTURE"));
    }
}
