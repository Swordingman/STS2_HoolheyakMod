using BaseLib.Abstracts;
using HoolheyakMod.Scripts.Cards;
using HoolheyakMod.Scripts.Utils;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using BaseLib.Patches.UI;
using HoolheyakMod.Code.Relics;

namespace HoolheyakMod.Code.Character;

public class HoolheyakMod : PlaceholderCharacterModel
{
    public const string CharacterId = "HoolheyakMod";

    public static readonly Color Color = new(40, 150, 190);
    public override Color NameColor => Color;

    public override Color EnergyLabelOutlineColor => new(40f / 255f, 150f / 255f, 190f / 255f, 1f);

    public override CharacterGender Gender => CharacterGender.Feminine;

    public override int StartingHp => 67;

    public static string GetVisualPath(int skinIndex)
    {
        return HoolheyakSkinState.Normalize(skinIndex) switch
        {
            1 => "res://HoolheyakMod/scenes/Hoolheyak_anim_skin1.tscn",
            2 => "res://HoolheyakMod/scenes/Hoolheyak_anim_skin2.tscn",
            _ => "res://HoolheyakMod/scenes/Hoolheyak_anim_skin0.tscn"
        };
    }
    public override string CustomVisualPath => GetVisualPath(HoolheyakConfig.CurrentSkinIndex);

    public override string CustomIconTexturePath => "res://HoolheyakMod/Icon.svg";
    public override string CustomIconPath => "res://HoolheyakMod/scenes/Hoolheyak_Icon.tscn";
    public override string CustomEnergyCounterPath => "res://HoolheyakMod/scenes/Hoolheyak_eneg.tscn";

    public static string GetRestSiteAnimPath(int skinIndex)
    {
        return HoolheyakSkinState.Normalize(skinIndex) switch
        {
            1 => "res://HoolheyakMod/scenes/Hoolheyak_Rest_Site_skin1.tscn",
            2 => "res://HoolheyakMod/scenes/Hoolheyak_Rest_Site_skin2.tscn",
            _ => "res://HoolheyakMod/scenes/Hoolheyak_Rest_Site_skin0.tscn"
        };
    }
    public override string CustomRestSiteAnimPath => GetRestSiteAnimPath(HoolheyakConfig.CurrentSkinIndex);

    public static string GetMerchantAnimPath(int skinIndex)
    {
        return HoolheyakSkinState.Normalize(skinIndex) switch
        {
            1 => "res://HoolheyakMod/scenes/Relaxed_Hoolheyak_anim_skin1.tscn",
            2 => "res://HoolheyakMod/scenes/Relaxed_Hoolheyak_anim_skin2.tscn",
            _ => "res://HoolheyakMod/scenes/Relaxed_Hoolheyak_anim_skin0.tscn"
        };
    }
    public override string CustomMerchantAnimPath => GetMerchantAnimPath(HoolheyakConfig.CurrentSkinIndex);

    // 多人模式手势
    public override string CustomArmPointingTexturePath => "res://HoolheyakMod/images/charui/hand_point.png";
    public override string CustomArmRockTexturePath => "res://HoolheyakMod/images/charui/hand_rock.png";
    public override string CustomArmPaperTexturePath => "res://HoolheyakMod/images/charui/hand_paper.png";
    public override string CustomArmScissorsTexturePath => "res://HoolheyakMod/images/charui/hand_scissors.png";

    public override string CustomCharacterSelectBg => "res://HoolheyakMod/scenes/Hoolheyak_bg.tscn";
    public override string CustomCharacterSelectIconPath => "res://HoolheyakMod/scenes/Hoolheyak_select.png";
    public override string CustomCharacterSelectLockedIconPath => "res://HoolheyakMod/scenes/locked_Hoolheyak_select.png";
    public override string CustomMapMarkerPath => "res://HoolheyakMod/Icon.svg";

    public override string CustomAttackSfx => "res://HoolheyakMod/audio/attack.wav";
    public override string CustomDeathSfx => "res://HoolheyakMod/audio/die.wav";

    public override RelicIconData? CustomYummyCookie => new RelicIconData(
        BigIconPath: "res://HoolheyakMod/images/relics/large/Cookies.png", 
        PackedIconPath: "res://HoolheyakMod/images/relics/large/Cookies.png",
        PackedIconOutlinePath: "res://HoolheyakMod/images/relics/large/Cookies.png"
    );

    public override string CharacterSelectSfx => HoolheyakConfig.CharacterVoice switch
    {
        VoiceLanguage.EN => "res://HoolheyakMod/audio/en.wav",
        VoiceLanguage.JP => "res://HoolheyakMod/audio/jp.wav",
        VoiceLanguage.KR => "res://HoolheyakMod/audio/kr.wav",
        _ => "res://HoolheyakMod/audio/cn.wav"
    };

    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

    public override CardPoolModel CardPool => ModelDb.CardPool<HoolheyakModCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<HoolheyakModRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<HoolheyakModPotionPool>();

    // 初始卡组
    public override IEnumerable<CardModel> StartingDeck
    {
        get
        {
            var customDeck = new List<CardModel>
            {
                ModelDb.Card<FeatherStrike>(),
                ModelDb.Card<ControlGroup>()
            };
            customDeck.AddRange(Enumerable.Repeat(ModelDb.Card<StrikeHoolheyak>(), 4));
            customDeck.AddRange(Enumerable.Repeat(ModelDb.Card<DefendHoolheyak>(), 4));

            return customDeck;
        }
    }

    // 初始遗物
    public override IReadOnlyList<RelicModel> StartingRelics
    {
        get
        {
            var relics = new System.Collections.Generic.List<RelicModel>
            {
                ModelDb.Relic<Bibliotheca>()
            };
            return relics;
        }
    }

    public override List<string> GetArchitectAttackVfx() => [
        VfxCmd.heavyBluntPath,
        VfxCmd.bluntPath,
        VfxCmd.slashPath,
        VfxCmd.slashPath,
        VfxCmd.heavyBluntPath
    ];

    public override CreatureAnimator? SetupCustomAnimationStates(MegaSprite controller)
    {
#if STS2_BETA
        return SetupAnimationState(
            controller,
            idleName: "idle_loop",
            deadName: "die",
            hitName: "hurt",
            attackName: "attack",
            castName: "cast"
        );
#else
        return null;
#endif
    }
}
