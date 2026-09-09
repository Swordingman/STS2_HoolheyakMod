using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class Revision : HoolheyakBaseCard
{
    public Revision() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        if (player == null) return;

        var hand = PileType.Hand.GetPile(player);
        if (hand.Cards.Count == 0) return;

        var prefs = new CardSelectorPrefs(new LocString("cards", "HOOLHEYAKMOD-REVISION.select_message"), 1);
        var selected = (await CardSelectCmd.FromHand(choiceContext, player, prefs, null, this)).ToList();
        if (selected.Count == 0) return;

        var sourceCard = selected[0];
        await CardCmd.Exhaust(choiceContext, sourceCard);

        var candidates = ModelDb.AllCards
            .Where(c => c.Type == sourceCard.Type
                     && c.Rarity != CardRarity.Token
                     && c.Rarity != CardRarity.Curse)
            .ToList();

        if (candidates.Count == 0) return;

        var generated = CardFactory.GetDistinctForCombat(
            player,
            candidates,
            1,
            player.RunState.Rng.CombatCardGeneration
        ).ToList();

        foreach (var card in generated)
        {
            // 升级后：生成的牌在本回合中耗能变为 0。
            if (IsUpgraded)
            {
                card.SetToFreeThisTurn();
            }

            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
        }
    }

    protected override void OnUpgrade()
    {
        // 无升级数值变化（升级效果体现在生成牌本回合 0 费）
    }
}
