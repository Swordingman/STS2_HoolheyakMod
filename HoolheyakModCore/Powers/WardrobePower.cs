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

    /// <summary>
    /// 是否把生成的牌变为本场战斗 0 费
    /// </summary>
    public bool MakeZeroCost { get; set; }

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
        DynamicVars["Wardrobe-MakeZeroCost"].BaseValue = MakeZeroCost ? 1m : 0m;
        InvokeDisplayAmountChanged();
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
                    && c.Type != CardType.Status
                    && c.Type != CardType.Curse
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

        foreach (var randomCard in generatedCards)
        {
            randomCard.AddKeyword(CardKeyword.Exhaust);

            if (MakeZeroCost)
                randomCard.SetToFreeThisCombat();

            await CardPileCmd.AddGeneratedCardToCombat(
                randomCard,
                PileType.Hand,
                player
            );
        }
    }
}
