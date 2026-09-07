using BaseLib.Extensions;
using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace HoolheyakMod.Scripts;

public class HoolheyakCardTags
{
    [CustomEnum]
    public static CardTag Variable;
}

public class HoolheyakKeywords
{
    [CustomEnum("ERUDITION")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Erudition;

    [CustomEnum("MEANDER")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Meander;

    [CustomEnum("ANALYSIS")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Analysis;

    [CustomEnum("DECONSTRUCTION")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Deconstruction;

    [CustomEnum("LIFT")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Lift;

    [CustomEnum("GRAVITY")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Gravity;

    [CustomEnum("PHASE")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Phase;
}
