/*
using BaseLib.Abstracts;
using HoolheyakMod.Code.Encounters;
using MegaCrit.Sts2.Core.Rewards;
using HoolheyakMod.Code.Relics;
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
using MegaCrit.Sts2.Core.Entities.Relics;

namespace HoolheyakMod.Scripts.Events;

public sealed class WaterLikeShapeEvent : CustomEventModel
{
    public override string? CustomInitialPortraitPath => "res://HoolheyakMod/images/events/water_like_shape.png";

    public override bool IsAllowed(IRunState runState) => true;

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var options = new List<EventOption>();

        options.Add(new EventOption(this, PrepareFight,
            new LocString("events", "HOOLHEYAKMOD-WATER_LIKE_SHAPE.pages.INITIAL.options.PREPARE_FIGHT.title"),
            new LocString("events", "HOOLHEYAKMOD-WATER_LIKE_SHAPE.pages.INITIAL.options.PREPARE_FIGHT.description"),
            "PREPARE_FIGHT", Array.Empty<IHoverTip>()));

        bool canRemove = Owner != null && Owner.Deck.Cards.Count > 0;
        options.Add(new EventOption(this, canRemove ? TravelTogether : null,
            new LocString("events", "HOOLHEYAKMOD-WATER_LIKE_SHAPE.pages.INITIAL.options.TRAVEL_TOGETHER.title"),
            new LocString("events", "HOOLHEYAKMOD-WATER_LIKE_SHAPE.pages.INITIAL.options.TRAVEL_TOGETHER.description"),
            "TRAVEL_TOGETHER", HoverTipFactory.FromRelic<PureWaterSpriteAssist>()));

        options.Add(new EventOption(this, MakeExcuse,
            new LocString("events", "HOOLHEYAKMOD-WATER_LIKE_SHAPE.pages.INITIAL.options.MAKE_EXCUSE.title"),
            new LocString("events", "HOOLHEYAKMOD-WATER_LIKE_SHAPE.pages.INITIAL.options.MAKE_EXCUSE.description"),
            "MAKE_EXCUSE", Array.Empty<IHoverTip>()));

        return options;
    }

    private Task PrepareFight()
    {
        if (Owner == null) return Task.CompletedTask;

        EnterCombatWithoutExitingEvent<MuelsyseEncounter>(
            [
                new PotionReward(Owner),
                new RelicReward(RelicRarity.Rare, Owner)
            ],
            shouldResumeAfterCombat: true
        );

        return Task.CompletedTask;
    }

    private async Task TravelTogether()
    {
        if (Owner == null) return;

        var cardToRemove = PickHighestRarityCard();
        if (cardToRemove != null)
        {
            await CardPileCmd.RemoveFromDeck([cardToRemove]);
        }

        await RelicCmd.Obtain(ModelDb.Relic<PureWaterSpriteAssist>().ToMutable(), Owner);
        SetEventFinished(PageDescription("TRAVELED_TOGETHER"));
    }

    private async Task MakeExcuse()
    {
        if (Owner == null) return;

        // 占位：获得一件通用遗物（TailCareKit）。后续可接入随机普通遗物。
        await RelicCmd.Obtain(ModelDb.Relic<TailCareKit>().ToMutable(), Owner);
        SetEventFinished(PageDescription("MADE_EXCUSE"));
    }

    private CardModel? PickHighestRarityCard()
    {
        if (Owner == null) return null;

        CardRarity[] priorities = [CardRarity.Rare, CardRarity.Uncommon, CardRarity.Common, CardRarity.Basic];

        foreach (var rarity in priorities)
        {
            var cards = Owner.Deck.Cards.Where(c => c.Rarity == rarity).ToList();
            if (cards.Count > 0)
            {
                return cards[new Random().Next(cards.Count)];
            }
        }

        return null;
    }
}
*/