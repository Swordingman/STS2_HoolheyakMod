using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class ParameterCorrection : HoolheyakBaseCard
{
    public ParameterCorrection() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Retain];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("ParameterCorrection-Magic", 1m)
    ];

    public override bool HasTurnEndInHandEffect => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayCastAnimation();

        // 打出时本身没有即时效果
        await Task.CompletedTask;
    }

    /// <summary>
    /// 回合结束时选择其他手牌，使其仅在本回合保留，并获得逶迤。
    /// </summary>
    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        var player = Owner;
        // Stable resolves this callback in Hand; Beta first moves turn-end cards
        // into Play. The engine already checked that the card ended the turn in Hand.
        if (player?.Creature == null || Pile?.Type is not (PileType.Hand or PileType.Play)) return;

        var hand = PileType.Hand.GetPile(player);
        bool CanRetain(CardModel card) => card != this && !card.ShouldRetainThisTurn;
        int maxCount = System.Math.Min(DynamicVars["ParameterCorrection-Magic"].IntValue,
            hand.Cards.Count(CanRetain));
        if (maxCount <= 0) return;

        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 0, maxCount);
        var selected = (await CardSelectCmd.FromHand(choiceContext, player, prefs, CanRetain, this))
            .Where(card => card.Pile?.Type == PileType.Hand && card.Owner == player && CanRetain(card))
            .ToList();
        if (selected.Count == 0) return;

        foreach (var card in selected)
            card.GiveSingleTurnRetain();

        await PowerCmd.Apply<MeanderPower>(choiceContext, player.Creature, 1, player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 保留手牌数量 1 -> 2
        DynamicVars["ParameterCorrection-Magic"].UpgradeValueBy(1);
    }
}
