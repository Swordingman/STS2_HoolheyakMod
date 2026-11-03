using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class DeepInquiry : HoolheyakBaseCard
{
    // 官方 AnyAlly 目标允许选择自己，也允许选择其他存活玩家。
    public DeepInquiry() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly, true)
    {
    }

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combat = Owner?.Creature.CombatState;
        var target = cardPlay.Target?.Player;
        if (combat == null || target == null || !combat.Players.Contains(target))
            return;

        await PlayCastAnimation();

        var candidates = target.Character.CardPool
            .GetUnlockedCards(target.UnlockState, CardMultiplayerConstraint.MultiplayerOnly)
            .Where(card => card.CanBeGeneratedInCombat
                && card.Type is CardType.Attack or CardType.Skill or CardType.Power)
            .ToList();
        if (candidates.Count == 0)
            return;

        // 选牌者可能不是打牌者；保持共享动作等待，使用原生玩家选择同步分发各自的选牌。
        var selectionContext = new BlockingPlayerChoiceContext();
        foreach (var player in combat.Players.ToArray())
        {
            if (!player.Creature.IsAlive || player.Creature.CombatState != combat)
                continue;

            var choices = CardFactory.GetDistinctForCombat(player, candidates, 3,
                player.RunState.Rng.CombatCardGeneration).ToList();
            if (choices.Count == 0)
                continue;

            var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);
            var selected = (await CardSelectCmd.FromSimpleGrid(selectionContext, choices, player, prefs))
                .FirstOrDefault();
            if (selected == null)
                continue;

            selected.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Hand, player);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
