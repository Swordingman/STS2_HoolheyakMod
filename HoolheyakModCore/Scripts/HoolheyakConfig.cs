using BaseLib.Config;

namespace HoolheyakMod.Scripts.Utils;

public enum VoiceLanguage
{
    CN,
    EN,
    JP,
    KR
}

[ConfigHoverTipsByDefault]
public sealed class HoolheyakConfig : SimpleModConfig
{
    [ConfigSection("Skin")]
    [ConfigHideInUI]
    public static int CurrentSkinIndex { get; set; } = 0;

    [ConfigHideInUI]
    public static int CurrentPreset { get; set; } = 0;

    [ConfigHideInUI]
    public static int CurrentDifficulty { get; set; } = 0;

    [ConfigSection("Voice Settings")]
    [ConfigHoverTip]
    public static VoiceLanguage CharacterVoice { get; set; } = VoiceLanguage.CN;
}
