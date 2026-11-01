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

    [CustomEnum("SLOUGH")]
    // 蜕生的条件效果已写在描述中；保留关键词提示，但不自动插入独立一行。
    [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Slough;
}
