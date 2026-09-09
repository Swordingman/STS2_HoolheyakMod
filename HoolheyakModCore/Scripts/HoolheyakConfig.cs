using BaseLib.Config;
using System.Collections.Generic;

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
    // 皮肤索引（0/1/2）：既可在 Mod 设置里填写，也由角色选择界面的皮肤选择器切换。
    // 通过 ModConfig.SaveDebounced 长效保存到 mod_configs/HoolheyakMod.cfg。
    [ConfigSection("Skin")]
    [ConfigHoverTip]
    public static int CurrentSkinIndex { get; set; } = 0;

    [ConfigHideInUI]
    public static List<int> EnabledChallenges { get; set; } = new();

    [ConfigHideInUI]
    public static int CurrentPreset { get; set; } = 0;

    [ConfigHideInUI]
    public static int CurrentDifficulty { get; set; } = 0;

    [ConfigSection("Voice Settings")]
    [ConfigHoverTip]
    public static VoiceLanguage CharacterVoice { get; set; } = VoiceLanguage.CN;
}
