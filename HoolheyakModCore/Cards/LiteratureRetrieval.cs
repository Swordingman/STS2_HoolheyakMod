using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class LiteratureRetrieval : HoolheyakBaseCard
{
    public LiteratureRetrieval() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        if (player == null || player.Creature?.CombatState == null)
            return;

        // 收集非本家、非诅咒/状态的随机卡作为“发现”池
        var candidates = ModelDb.AllCardPools
            .SelectMany(pool => pool.AllCardIds)
            .Select(id => ModelDb.GetById<CardModel>(id))
            .Where(card => card != null
                && card is not HoolheyakBaseCard
                && card is not Archive
                && card.Type != CardType.Curse
                && card.Type != CardType.Status)
            .ToList();

        if (candidates.Count == 0)
            return;

        var choices = CardFactory.GetDistinctForCombat(player, candidates, 3, player.RunState.Rng.CombatCardGeneration).ToList();
        if (choices.Count == 0)
            return;

        var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
        var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, choices, player, prefs)).ToList();
        if (selected.Count == 0)
            return;

        var chosen = selected[0];

        // 升级后选中的牌本回合 0 费
        if (IsUpgraded)
        {
            chosen.SetToFreeThisTurn();
        }

        var hand = PileType.Hand.GetPile(player);
        if (hand.Cards.Count < CardPile.MaxCardsInHand)
        {
            await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, player);
        }
        else
        {
            await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Discard, player);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级效果由 IsUpgraded 控制：选中牌本回合 0 费
    }
}
