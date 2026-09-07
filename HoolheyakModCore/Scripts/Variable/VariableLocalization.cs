using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using System;

namespace HoolheyakMod.Code.Variables;

public static class VariableLocalization
{
    public static string GetChoiceDescription(CardModel sourceCard, int index)
    {
        LocString loc = new("cards", $"{sourceCard.Id.Entry}.variable.{index}");

        if (!loc.Exists())
            throw new InvalidOperationException($"Missing variable localization: {sourceCard.Id.Entry}.variable.{index}");

        sourceCard.DynamicVars.AddTo(loc);
        return loc.GetFormattedText();
    }
}