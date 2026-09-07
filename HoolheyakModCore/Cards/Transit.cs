using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
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

        // Java 状态牌被选择时直接施加对应相位 Power。
        if (chosen is ConjunctionCard)
            await PowerCmd.Apply<ConjunctionPower>(choiceContext, creature, 1, creature, this);
        else if (chosen is OppositionCard)
            await PowerCmd.Apply<OppositionPower>(choiceContext, creature, 1, creature, this);
        else if (chosen is QuincunxCard)
            await PowerCmd.Apply<QuincunxPower>(choiceContext, creature, 1, creature, this);
        else if (chosen is SextileCard)
            await PowerCmd.Apply<SextilePower>(choiceContext, creature, 1, creature, this);
        else if (chosen is SquareCard)
            await PowerCmd.Apply<SquarePower>(choiceContext, creature, 1, creature, this);
        else if (chosen is TrineCard)
            await PowerCmd.Apply<TrinePower>(choiceContext, creature, 1, creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
