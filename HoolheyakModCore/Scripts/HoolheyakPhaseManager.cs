using System;
using System.Threading.Tasks;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Scripts.Utils;

public static class HoolheyakPhaseManager
{
    public static async Task ApplyPhase(PlayerChoiceContext choiceContext, Creature owner, PowerModel newPhase)
    {
        if (newPhase == null)
            return;

        // 清除旧相位
        await PowerCmd.Remove<ConjunctionPower>(owner);
        await PowerCmd.Remove<QuincunxPower>(owner);
        await PowerCmd.Remove<SextilePower>(owner);
        await PowerCmd.Remove<TrinePower>(owner);
        await PowerCmd.Remove<SquarePower>(owner);
        await PowerCmd.Remove<OppositionPower>(owner);

        // 施加新相位
        await PowerCmd.Apply(choiceContext, newPhase, owner, 1, owner, null);
    }

    public static PowerModel GetRandomPhase(Creature owner)
    {
        int index = Random.Shared.Next(6);

        return index switch
        {
            0 => ModelDb.Power<ConjunctionPower>().ToMutable(),
            1 => ModelDb.Power<QuincunxPower>().ToMutable(),
            2 => ModelDb.Power<SextilePower>().ToMutable(),
            3 => ModelDb.Power<TrinePower>().ToMutable(),
            4 => ModelDb.Power<SquarePower>().ToMutable(),
            _ => ModelDb.Power<OppositionPower>().ToMutable()
        };
    }
}