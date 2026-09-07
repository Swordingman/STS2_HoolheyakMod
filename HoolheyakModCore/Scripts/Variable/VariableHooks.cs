using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Variables;

public interface IVariableTriggeredPower
{
    Task OnVariableTriggered(
        PlayerChoiceContext choiceContext,
        CardModel sourceCard
    );
}

public static class VariableHooks
{
    public static async Task OnVariableTriggered(
        PlayerChoiceContext choiceContext,
        CardModel sourceCard)
    {
        var powers = sourceCard.Owner.Creature.Powers
            .OfType<IVariableTriggeredPower>()
            .ToList();

        foreach (IVariableTriggeredPower power in powers)
        {
            await power.OnVariableTriggered(
                choiceContext,
                sourceCard
            );
        }
    }
}