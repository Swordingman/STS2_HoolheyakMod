using BaseLib.Abstracts;
using HoolheyakMod.Code.Encounters;
using HoolheyakMod.Code.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Events;

public sealed class WaterLikeShapeEvent : CustomEventModel
{
    private CardModel? _tradeCard;
    public override bool IsShared => true;
    public override string? CustomInitialPortraitPath => "res://HoolheyakMod/images/events/water_like_shape.png";
    public override EncounterModel CanonicalEncounter => ModelDb.Encounter<MuelsyseEncounter>();
    protected override IEnumerable<DynamicVar> CanonicalVars => [new StringVar("TradeCard")];
    public override bool IsAllowed(IRunState runState) => runState.CurrentActIndex is 1 or 2 &&
        runState.Players.Count == 1 &&
        runState.Players[0].Character is HoolheyakMod.Code.Character.HoolheyakMod;

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        _tradeCard = PickHighestRarityCard();
        ((StringVar)DynamicVars["TradeCard"]).StringValue = _tradeCard?.Title ?? "—";
        return [
            MakeOption(PrepareFight, "PREPARE_FIGHT"),
            MakeOption(_tradeCard != null ? TravelTogether : null, "TRAVEL_TOGETHER",
                HoverTipFactory.FromRelic<PureWaterSpriteAssist>()),
            MakeOption(MakeExcuse, "MAKE_EXCUSE")
        ];
    }

    private EventOption MakeOption(Func<Task>? action, string key, IEnumerable<IHoverTip>? tips = null) =>
        new(this, action, new LocString("events", $"{Id.Entry}.pages.INITIAL.options.{key}.title"),
            new LocString("events", $"{Id.Entry}.pages.INITIAL.options.{key}.description"),
            key, tips ?? Array.Empty<IHoverTip>());

    private Task PrepareFight()
    {
        if (Owner == null) return Task.CompletedTask;
        EnterCombatWithoutExitingEvent<MuelsyseEncounter>(
            [new GoldReward(60, Owner), new PotionReward(Owner), new RelicReward(RelicRarity.Rare, Owner)],
            shouldResumeAfterCombat: true);
        return Task.CompletedTask;
    }

    public override Task Resume(AbstractRoom previousRoom)
    {
        SetEventFinished(PageDescription("COMBAT_DONE"));
        return Task.CompletedTask;
    }

    private async Task TravelTogether()
    {
        if (Owner == null || _tradeCard == null || !Owner.Deck.Cards.Contains(_tradeCard)) return;
        await CardPileCmd.RemoveFromDeck([_tradeCard]);
        await RelicCmd.Obtain<PureWaterSpriteAssist>(Owner);
        SetEventFinished(PageDescription("TRAVELED_TOGETHER"));
    }

    private async Task MakeExcuse()
    {
        if (Owner == null) return;
        var relic = Owner.RelicGrabBag.PullFromFront(RelicRarity.Common, Owner.RunState);
        if (relic != null) await RelicCmd.Obtain(relic.ToMutable(), Owner);
        SetEventFinished(PageDescription("MADE_EXCUSE"));
    }

    private CardModel? PickHighestRarityCard()
    {
        if (Owner == null) return null;
        CardRarity[] priorities = [CardRarity.Rare, CardRarity.Uncommon, CardRarity.Common, CardRarity.Basic];
        foreach (var rarity in priorities)
        {
            var cards = Owner.Deck.Cards.Where(c => c.Rarity == rarity && c.IsRemovable).ToList();
            if (cards.Count > 0) return Rng.NextItem(cards);
        }
        return null;
    }
}
