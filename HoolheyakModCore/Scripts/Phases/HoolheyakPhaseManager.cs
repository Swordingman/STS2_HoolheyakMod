using System.Threading.Tasks;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

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

    public static async Task ApplyPhase(PlayerChoiceContext choiceContext, Creature owner, PhaseType phase)
    {
        await RemoveCurrentPhase(owner);
        await ApplyPhasePower(choiceContext, owner, phase);

        PhaseVisualManager.Play(owner, phase);
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

    private static async Task ApplyPhasePower(PlayerChoiceContext choiceContext, Creature owner, PhaseType phase)
    {
        switch (phase)
        {
            case PhaseType.Conjunction:
                await PowerCmd.Apply<ConjunctionPower>(choiceContext, owner, 1, owner, null);
                break;

            case PhaseType.Quincunx:
                await PowerCmd.Apply<QuincunxPower>(choiceContext, owner, 1, owner, null);
                break;

            case PhaseType.Sextile:
                await PowerCmd.Apply<SextilePower>(choiceContext, owner, 1, owner, null);
                break;

            case PhaseType.Trine:
                await PowerCmd.Apply<TrinePower>(choiceContext, owner, 1, owner, null);
                break;

            case PhaseType.Square:
                await PowerCmd.Apply<SquarePower>(choiceContext, owner, 1, owner, null);
                break;

            case PhaseType.Opposition:
                await PowerCmd.Apply<OppositionPower>(choiceContext, owner, 1, owner, null);
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