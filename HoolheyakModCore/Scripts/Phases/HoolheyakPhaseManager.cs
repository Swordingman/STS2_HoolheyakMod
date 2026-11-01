using System.Threading.Tasks;
using System;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Scripts.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Scripts.Phases;

public enum PhaseType
{
    Conjunction,
    Quincunx,
    Sextile,
    Trine,
    Square,
    Opposition
}

public static class HoolheyakPhaseManager
{
    private static readonly PhaseType[] AllPhases =
    [
        PhaseType.Conjunction,
        PhaseType.Quincunx,
        PhaseType.Sextile,
        PhaseType.Trine,
        PhaseType.Square,
        PhaseType.Opposition
    ];

    private static readonly PhaseType[] GoodPhases =
    [
        PhaseType.Sextile,
        PhaseType.Trine
    ];

    public static bool IsPhaseOwner(Creature? creature) =>
        creature?.Player?.Character is HoolheyakMod.Code.Character.HoolheyakMod;

    public static async Task ApplyPhase(PlayerChoiceContext choiceContext, Creature owner, PhaseType phase,
        CardModel? source = null)
    {
        if (!IsPhaseOwner(owner))
            return;
        if (!Enum.IsDefined(phase))
            throw new ArgumentOutOfRangeException(nameof(phase));

        await RemoveCurrentPhase(owner);
        await ApplyPhasePower(choiceContext, owner, phase, source);

        // 拱相抽牌可能触发另一张相位牌；保留最后实际生效的相位特效。
        if (GetCurrentPhase(owner) == phase)
            PhaseVisualManager.Play(owner, phase);
    }

    public static PhaseType? GetCurrentPhase(Creature owner)
    {
        if (owner.GetPower<ConjunctionPower>() != null) return PhaseType.Conjunction;
        if (owner.GetPower<QuincunxPower>() != null) return PhaseType.Quincunx;
        if (owner.GetPower<SextilePower>() != null) return PhaseType.Sextile;
        if (owner.GetPower<TrinePower>() != null) return PhaseType.Trine;
        if (owner.GetPower<SquarePower>() != null) return PhaseType.Square;
        if (owner.GetPower<OppositionPower>() != null) return PhaseType.Opposition;
        return null;
    }

    public static bool TryGetPhase(CardModel card, out PhaseType phase)
    {
        PhaseType? selected = card switch
        {
            ConjunctionCard => PhaseType.Conjunction,
            QuincunxCard => PhaseType.Quincunx,
            SextileCard => PhaseType.Sextile,
            TrineCard => PhaseType.Trine,
            SquareCard => PhaseType.Square,
            OppositionCard => PhaseType.Opposition,
            _ => null
        };
        phase = selected.GetValueOrDefault();
        return selected.HasValue;
    }

    private static async Task RemoveCurrentPhase(Creature owner)
    {
        await PowerCmd.Remove<ConjunctionPower>(owner);
        await PowerCmd.Remove<QuincunxPower>(owner);
        await PowerCmd.Remove<SextilePower>(owner);
        await PowerCmd.Remove<TrinePower>(owner);
        await PowerCmd.Remove<SquarePower>(owner);
        await PowerCmd.Remove<OppositionPower>(owner);
    }

    private static async Task ApplyPhasePower(PlayerChoiceContext choiceContext, Creature owner, PhaseType phase,
        CardModel? source)
    {
        switch (phase)
        {
            case PhaseType.Conjunction:
                await PowerCmd.Apply<ConjunctionPower>(choiceContext, owner, 1, owner, source);
                break;

            case PhaseType.Quincunx:
                await PowerCmd.Apply<QuincunxPower>(choiceContext, owner, 1, owner, source);
                break;

            case PhaseType.Sextile:
                await PowerCmd.Apply<SextilePower>(choiceContext, owner, 1, owner, source);
                break;

            case PhaseType.Trine:
                await PowerCmd.Apply<TrinePower>(choiceContext, owner, 1, owner, source);
                break;

            case PhaseType.Square:
                await PowerCmd.Apply<SquarePower>(choiceContext, owner, 1, owner, source);
                break;

            case PhaseType.Opposition:
                await PowerCmd.Apply<OppositionPower>(choiceContext, owner, 1, owner, source);
                break;
        }
    }

    public static PhaseType GetRandomPhase(Creature owner)
    {
        return owner.Player!.RunState.Rng.CombatCardSelection.NextItem(AllPhases);
    }

    public static PhaseType GetRandomGoodPhase(Creature owner)
    {
        return owner.Player!.RunState.Rng.CombatCardSelection.NextItem(GoodPhases);
    }
}
