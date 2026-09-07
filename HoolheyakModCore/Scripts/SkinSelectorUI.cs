using BaseLib.Config;
using Godot;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Networking;
using HoolheyakMod.Scripts.Utils;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using System;
using System.Collections.Generic;

namespace HoolheyakMod.Scripts.Utils;

public partial class SkinSelectorUI : Control
{
    // 皮肤选择器
    private TextureButton? _leftBtn;
    private TextureButton? _rightBtn;
    private Label? _nameLabel;
    private Node2D? _modelPlaceholder;

    // 挑战/选项面板
    private Button? _toggleMenuBtn;
    private PanelContainer? _challengeMenuPanel;
    private Label? _challengeTitleLabel;
    private VBoxContainer? _challengesVBox;

    private const int MaxSkins = 3;
    private const string ChallengeToggleKey = "HOOLHEYAKMOD_CHALLENGE_TOGGLE_BTN";
    private const string ChallengeTitleKey = "HOOLHEYAKMOD_CHALLENGE_TITLE";
    private const string ChallengeOptionPrefix = "HOOLHEYAKMOD_CHALLENGE_OPTION_";

    private Code.Character.HoolheyakMod? _currentCharacter;

    public override void _Ready()
    {
        if (IsInsideMultiplayerLoadScreen())
        {
            GetNode<TextureRect>("TextureRect").Visible = true;
            GetNode<Control>("SkinSelectorPanel").Visible = false;
            GetNode<Control>("ChallengeSelectorPanel").Visible = false;
            return;
        }

        // 皮肤节点
        _leftBtn = GetNode<TextureButton>("SkinSelectorPanel/ArrowContainer/LeftArrow");
        _rightBtn = GetNode<TextureButton>("SkinSelectorPanel/ArrowContainer/RightArrow");
        _nameLabel = GetNode<Label>("SkinSelectorPanel/SkinName");
        _modelPlaceholder = GetNode<Node2D>(
            "SkinSelectorPanel/SubViewportContainer/SubViewport/ModelInstancePlaceholder");

        // 挑战/选项节点，与 Hoolheyak_bg.tscn 一一对应
        _toggleMenuBtn = GetNode<Button>("ChallengeSelectorPanel/ToggleMenuButton");
        _challengeMenuPanel = GetNode<PanelContainer>("ChallengeSelectorPanel/ChallengeMenuPanel");
        _challengeTitleLabel = GetNode<Label>(
            "ChallengeSelectorPanel/ChallengeMenuPanel/VBoxContainer/ChallengeLabel");
        _challengesVBox = GetNode<VBoxContainer>(
            "ChallengeSelectorPanel/ChallengeMenuPanel/VBoxContainer/ScrollContainer/OptionsVBox");

        var leftArrowTex = GD.Load<Texture2D>("res://images/packed/common_ui/settings_tiny_left_arrow.png");
        var rightArrowTex = GD.Load<Texture2D>("res://images/packed/common_ui/settings_tiny_right_arrow.png");

        if (_leftBtn != null) _leftBtn.TextureNormal = leftArrowTex;
        if (_rightBtn != null) _rightBtn.TextureNormal = rightArrowTex;

        if (_leftBtn != null) _leftBtn.Pressed += OnLeftPressed;
        if (_rightBtn != null) _rightBtn.Pressed += OnRightPressed;

        _currentCharacter = ModelDb.Character<Code.Character.HoolheyakMod>();

        InitChallengeUI();
        UpdateUI();
    }

    private void InitChallengeUI()
    {
        if (_toggleMenuBtn != null)
            _toggleMenuBtn.Text = GetLocalizedText("characters", ChallengeToggleKey, "挑战选项");

        if (_challengeTitleLabel != null)
            _challengeTitleLabel.Text = GetLocalizedText("characters", ChallengeTitleKey, "挑战设置");

        if (_challengesVBox != null)
        {
            // 防止场景因热重载/重复初始化出现重复选项。
            foreach (Node child in _challengesVBox.GetChildren())
            {
                _challengesVBox.RemoveChild(child);
                child.QueueFree();
            }

            int index = 1;
            while (true)
            {
                string locKey = $"{ChallengeOptionPrefix}{index}";
                if (!LocString.Exists("characters", locKey))
                    break;

                var checkBox = new CheckBox
                {
                    Text = new LocString("characters", locKey).GetFormattedText(),
                    ButtonPressed = HoolheyakConfig.EnabledChallenges.Contains(index)
                };
                checkBox.AddThemeFontSizeOverride("font_size", 24);

                int challengeId = index;
                checkBox.Toggled += isToggled =>
                {
                    if (isToggled && !HoolheyakConfig.EnabledChallenges.Contains(challengeId))
                        HoolheyakConfig.EnabledChallenges.Add(challengeId);
                    else if (!isToggled)
                        HoolheyakConfig.EnabledChallenges.Remove(challengeId);

                    ModConfig.SaveDebounced<HoolheyakConfig>();
                };

                _challengesVBox.AddChild(checkBox);
                index++;
            }
        }

        if (_toggleMenuBtn != null && _challengeMenuPanel != null)
            _toggleMenuBtn.Pressed += () => _challengeMenuPanel.Visible = !_challengeMenuPanel.Visible;
    }

    private static string GetLocalizedText(string table, string key, string fallback)
    {
        return LocString.Exists(table, key) ? new LocString(table, key).GetFormattedText() : fallback;
    }

    private void OnLeftPressed()
    {
        HoolheyakConfig.CurrentSkinIndex--;
        if (HoolheyakConfig.CurrentSkinIndex < 0)
            HoolheyakConfig.CurrentSkinIndex = MaxSkins - 1;

        ModConfig.SaveDebounced<HoolheyakConfig>();
        UpdateUI();
        HoolheyakSkinNetwork.PublishLocalSkin();
    }

    private void OnRightPressed()
    {
        HoolheyakConfig.CurrentSkinIndex++;
        if (HoolheyakConfig.CurrentSkinIndex >= MaxSkins)
            HoolheyakConfig.CurrentSkinIndex = 0;

        ModConfig.SaveDebounced<HoolheyakConfig>();
        UpdateUI();
        HoolheyakSkinNetwork.PublishLocalSkin();
    }

    private void UpdateUI()
    {
        if (_currentCharacter == null || _nameLabel == null || _modelPlaceholder == null)
            return;

        int skinIndex = HoolheyakSkinState.Normalize(HoolheyakConfig.CurrentSkinIndex);
        HoolheyakConfig.CurrentSkinIndex = skinIndex;

        string currentSkinKey = $"HOOLHEYAKMOD-HOOLHEYAK_MOD.skin{skinIndex}";
        _nameLabel.Text = LocString.Exists("characters", currentSkinKey)
            ? new LocString("characters", currentSkinKey).GetFormattedText()
            : $"Missing Loc: {currentSkinKey}";

        foreach (Node child in _modelPlaceholder.GetChildren())
        {
            _modelPlaceholder.RemoveChild(child);
            child.QueueFree();
        }

        string modelPath = _currentCharacter.CustomVisualPath;
        if (!Godot.FileAccess.FileExists(modelPath))
        {
            GD.PushWarning($"[HoolheyakMod UI] 找不到皮肤场景: {modelPath}");
            return;
        }

        PackedScene? skinScene = GD.Load<PackedScene>(modelPath);
        if (skinScene == null)
            return;

        Node2D skinInstance = skinScene.Instantiate<Node2D>();
        _modelPlaceholder.AddChild(skinInstance);
        skinInstance.Position = Vector2.Zero;
        PlayIdleAnimation(skinInstance);
    }

    private void PlayIdleAnimation(Node rootNode)
    {
        var queue = new Queue<Node>();
        queue.Enqueue(rootNode);

        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();

            if (current is NCreatureVisuals visuals)
            {
                try
                {
#if STS2_BETA
                    visuals.SpineAnimation.SetAnimation("idle_loop");
#else
                    MegaTrackEntry? entry = visuals.SpineAnimation.SetAnimation("idle_loop");
                    if (entry != null)
                    {
                        entry.SetLoop(true);
                        entry.SetTimeScale(MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextFloat(0.9f, 1.1f));

                        float animationEnd = entry.GetAnimationEnd();
                        if (animationEnd > 0f)
                        {
                            float offset = MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextFloat(-0.5f, 0.5f);
                            entry.SetTrackTime((animationEnd + offset) % animationEnd);
                        }
                    }
#endif
                    return;
                }
                catch (Exception e)
                {
                    GD.PushWarning($"[HoolheyakMod UI] NCreatureVisuals 播放报错: {e.Message}");
                }
            }

            if (current.GetClass() == "SpineSprite")
            {
                try
                {
                    var animState = current.Call("get_animation_state").AsGodotObject();
                    if (animState != null)
                    {
                        animState.Call("set_animation", "idle_loop", true, 0);
                        return;
                    }
                }
                catch (Exception e)
                {
                    GD.PushWarning($"[HoolheyakMod UI] SpineSprite 播放报错: {e.Message}");
                }
            }

            if (current is AnimationPlayer animPlayer && animPlayer.HasAnimation("idle_loop"))
            {
                animPlayer.Play("idle_loop");
                return;
            }

            if (current.HasMethod("set_animation"))
            {
                try
                {
                    current.Call("set_animation", "idle_loop", true, 0);
                    return;
                }
                catch { }
            }

            foreach (Node child in current.GetChildren())
                queue.Enqueue(child);
        }

        GD.PushWarning("[HoolheyakMod UI] 未找到可播放 idle_loop 的动画节点。");
    }

    private bool IsInsideMultiplayerLoadScreen()
    {
        Node? current = this;
        while (current != null)
        {
            if (current is NMultiplayerLoadGameScreen)
                return true;
            current = current.GetParent();
        }
        return false;
    }
}
