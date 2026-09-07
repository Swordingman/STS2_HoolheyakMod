using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Scripts.Cards;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Localization;

namespace HoolheyakMod.Scripts.Potions;

[Pool(typeof(HoolheyakModPotionPool))]
public class AstrolabeResidue : CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    public override string? CustomPackedImagePath => $"res://HoolheyakMod/images/potions/{nameof(AstrolabeResidue)}.png";
    public override string? CustomPackedOutlinePath => $"res://HoolheyakMod/images/potions/{nameof(AstrolabeResidue)}.png";

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
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

        var prefs = new CardSelectorPrefs(new LocString("potions", "HOOLHEYAKMOD-ASTROLABE_RESIDUE.select_message"), 1);
        var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, choices, player, prefs)).ToList();
        if (selected.Count == 0)
            return;

        var creature = player.Creature;
        var chosen = selected[0];

        if (chosen is ConjunctionCard)
            await PowerCmd.Apply<ConjunctionPower>(choiceContext, creature, 1, creature, null);
        else if (chosen is OppositionCard)
            await PowerCmd.Apply<OppositionPower>(choiceContext, creature, 1, creature, null);
        else if (chosen is QuincunxCard)
            await PowerCmd.Apply<QuincunxPower>(choiceContext, creature, 1, creature, null);
        else if (chosen is SextileCard)
            await PowerCmd.Apply<SextilePower>(choiceContext, creature, 1, creature, null);
        else if (chosen is SquareCard)
            await PowerCmd.Apply<SquarePower>(choiceContext, creature, 1, creature, null);
        else if (chosen is TrineCard)
            await PowerCmd.Apply<TrinePower>(choiceContext, creature, 1, creature, null);
    }
}
