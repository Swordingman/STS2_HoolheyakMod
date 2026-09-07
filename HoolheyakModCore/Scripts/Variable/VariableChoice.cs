using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Variables;

public sealed class VariableChoice
{
    public Func<PlayerChoiceContext, Task> Effect { get; }

    public VariableChoice(Func<PlayerChoiceContext, Task> effect)
    {
        Effect = effect;
    }
}