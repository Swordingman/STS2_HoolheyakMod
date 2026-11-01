using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class WardrobePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(WardrobePower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(WardrobePower)}.png";

    public int ZeroCostStacks { get; private set; }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Wardrobe-MakeZeroCost", 0m)
    ];

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        RefreshDynamicVars();
        return Task.CompletedTask;
    }

    private void RefreshDynamicVars()
    {
        if (DynamicVars == null)
            return;
        DynamicVars["Wardrobe-MakeZeroCost"].BaseValue = ZeroCostStacks;
        InvokeDisplayAmountChanged();
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext context, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (ReferenceEquals(power, this))
        {
            if (amount > 0 && cardSource is HoolheyakMod.Scripts.Cards.Wardrobe { IsUpgraded: true })
                ZeroCostStacks += (int)amount;
            ZeroCostStacks = System.Math.Min(ZeroCostStacks, System.Math.Max(0, Amount));
            RefreshDynamicVars();
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player == null || cardPlay.Card.Owner?.Creature != Owner || Amount <= 0)
            return;

        var card = cardPlay.Card;

        // 状态和诅咒不算“其他颜色牌”
        if (card.Type == CardType.Curse || card.Type == CardType.Status)
            return;

        var hoolheyakPool = ModelDb.CardPool<HoolheyakModCardPool>();

        // 本颜色牌不触发
        if (card.Pool == hoolheyakPool)
            return;

        Flash();

        var player = Owner.Player;

        // 只从本颜色牌中随机
        var candidates = ModelDb.AllCards
            .Where(c => c.Pool == hoolheyakPool
                    && c.CanBeGeneratedInCombat
                    && c.Type is CardType.Attack or CardType.Skill or CardType.Power
                    && c.Rarity != CardRarity.Token)
            .ToList();

        if (candidates.Count == 0)
            return;

        var generatedCards = CardFactory.GetDistinctForCombat(
            player,
            candidates,
            (int)Amount,
            player.RunState.Rng.CombatCardGeneration
        ).ToList();

        for (int i = 0; i < generatedCards.Count; i++)
        {
            var randomCard = generatedCards[i];
            randomCard.AddKeyword(CardKeyword.Exhaust);

            if (i < ZeroCostStacks)
                randomCard.SetToFreeThisCombat();

            await CardPileCmd.AddGeneratedCardToCombat(
                randomCard,
                PileType.Hand,
                player
            );
        }
    }
}
