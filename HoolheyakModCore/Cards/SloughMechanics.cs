using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Scripts.Cards;

internal static class SloughMechanics
{
    public static List<PowerModel> Debuffs(Creature creature) =>
        creature.Powers.Where(power => power.TypeForCurrentAmount == PowerType.Debuff).ToList();

    public static bool Active(Creature creature) =>
        creature.Powers.Any(power => power.TypeForCurrentAmount == PowerType.Debuff);

    public static async Task<CardModel?> SelectHandCard(
        PlayerChoiceContext context, Player player, CardModel source)
    {
        if (PileType.Hand.GetPile(player).Cards.Count == 0)
            return null;

        var prefs = new CardSelectorPrefs(
            new LocString("cards", "HOOLHEYAKMOD-SLOUGH_EXHAUST.select_message"), 1);
        return (await CardSelectCmd.FromHand(context, player, prefs, null, source))
            .FirstOrDefault();
    }

    public static async Task ChangeRandomDebuff(
        PlayerChoiceContext context, Creature creature, CardModel source, int delta)
    {
        var debuffs = Debuffs(creature);
        if (debuffs.Count == 0)
            return;

        var rng = creature.Player?.RunState.Rng.CombatCardSelection;
        int index = rng?.NextInt(debuffs.Count) ?? 0;
        var power = debuffs[index];

        await ChangeDebuff(context, power, source, delta);
    }

    public static async Task ChangeDebuff(
        PlayerChoiceContext context, PowerModel power, CardModel source, int delta)
    {
        if (!power.Owner.Powers.Contains(power) || power.TypeForCurrentAmount != PowerType.Debuff)
            return;

        // Negative Strength/Dexterity are debuffs too; reducing them moves toward zero.
        int offset = power.Amount < 0 ? -delta : delta;
        // Run the amount-change hooks even for the last stack. Dark Shackles uses
        // these hooks to return its Strength penalty before the power is removed.
        await PowerCmd.ModifyAmount(context, power, offset, power.Owner, source);
    }

    public static async Task<bool> RemoveDebuff(
        PlayerChoiceContext context, PowerModel power, CardModel source)
    {
        var owner = power.Owner;
        if (!owner.Powers.Contains(power) || power.TypeForCurrentAmount != PowerType.Debuff)
            return false;

        if (power.Amount == 0)
            await PowerCmd.Remove(power);
        else
            await PowerCmd.ModifyAmount(context, power, -power.Amount, owner, source);

        return !owner.Powers.Contains(power);
    }
}
