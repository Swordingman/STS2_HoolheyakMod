using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Scripts.Phases;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Cards;

[Pool(typeof(HoolheyakModCardPool))]
public class Transit : HoolheyakBaseCard
{
    public Transit() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayCastAnimation();

        var player = Owner;
        if (player == null || player.Creature?.CombatState == null)
            return;

        var combatState = player.Creature.CombatState;
        var choices = new List<CardModel>
        {
            combatState.CreateCard<ConjunctionCard>(player),
            combatState.CreateCard<QuincunxCard>(player),
            combatState.CreateCard<SextileCard>(player),
            combatState.CreateCard<TrineCard>(player),
            combatState.CreateCard<SquareCard>(player),
            combatState.CreateCard<OppositionCard>(player)
        };

        var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
        var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, choices, player, prefs)).ToList();
        if (selected.Count == 0)
            return;

        var chosen = selected[0];
        var creature = player.Creature;

        if (HoolheyakPhaseManager.TryGetPhase(chosen, out var phase))
            await HoolheyakPhaseManager.ApplyPhase(choiceContext, creature, phase, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
