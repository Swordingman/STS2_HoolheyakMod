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
        new GoldVar(50)
    ];

    public override bool IsAllowed(IRunState runState) => true;

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var options = new List<EventOption>();

        options.Add(new EventOption(this, PoliteReply,
            new LocString("events", "HOOLHEYAKMOD-INTERSECTION.pages.INITIAL.options.POLITE_REPLY.title"),
            new LocString("events", "HOOLHEYAKMOD-INTERSECTION.pages.INITIAL.options.POLITE_REPLY.description"),
            "POLITE_REPLY", Array.Empty<IHoverTip>()));

        options.Add(new EventOption(this, Quarrel,
            new LocString("events", "HOOLHEYAKMOD-INTERSECTION.pages.INITIAL.options.QUARREL.title"),
            new LocString("events", "HOOLHEYAKMOD-INTERSECTION.pages.INITIAL.options.QUARREL.description"),
            "QUARREL", Array.Empty<IHoverTip>()));

        options.Add(new EventOption(this, SeekHelp,
            new LocString("events", "HOOLHEYAKMOD-INTERSECTION.pages.INITIAL.options.SEEK_HELP.title"),
            new LocString("events", "HOOLHEYAKMOD-INTERSECTION.pages.INITIAL.options.SEEK_HELP.description"),
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
            var card = upgradable[new Random().Next(upgradable.Count)];
            CardCmd.Upgrade(card);
        }

        SetEventFinished(PageDescription("POLITE_REPLY_DONE"));
    }

    private async Task Quarrel()
    {
        if (Owner == null) return;

        // 失去 5 点生命（进阶 15+ 为 8，这里先用固定 5，后续可接 AscensionHelper）
        #if STS2_BETA
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            5,
            MegaCrit.Sts2.Core.ValueProps.ValueProp.Unblockable,
            (CardModel?)null!,
            null);
        #else
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            5,
            MegaCrit.Sts2.Core.ValueProps.ValueProp.Unblockable,
            (CardModel?)null!);
        #endif

        var removable = Owner.Deck.Cards.ToList();
        if (removable.Count > 0)
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
