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
    private TextureButton? _leftBtn;
    private TextureButton? _rightBtn;
    private Label? _nameLabel;
    private Node2D? _modelPlaceholder;

    private const int MaxSkins = 3;
    private Code.Character.HoolheyakMod? _currentCharacter;

    public override void _Ready()
    {
        if (IsInsideMultiplayerLoadScreen())
        {
            GetNode<TextureRect>("TextureRect").Visible = true;
            GetNode<Control>("SkinSelectorPanel").Visible = false;
            return;
        }

        _leftBtn = GetNode<TextureButton>("SkinSelectorPanel/ArrowContainer/LeftArrow");
        _rightBtn = GetNode<TextureButton>("SkinSelectorPanel/ArrowContainer/RightArrow");
        _nameLabel = GetNode<Label>("SkinSelectorPanel/SkinName");
        _modelPlaceholder = GetNode<Node2D>("SkinSelectorPanel/SubViewportContainer/SubViewport/ModelInstancePlaceholder");

        var leftArrowTex = GD.Load<Texture2D>("res://images/packed/common_ui/settings_tiny_left_arrow.png");
        var rightArrowTex = GD.Load<Texture2D>("res://images/packed/common_ui/settings_tiny_right_arrow.png");

        if (_leftBtn != null) _leftBtn.TextureNormal = leftArrowTex;
        if (_rightBtn != null) _rightBtn.TextureNormal = rightArrowTex;

        if (_leftBtn != null) _leftBtn.Pressed += OnLeftPressed;
        if (_rightBtn != null) _rightBtn.Pressed += OnRightPressed;

        _currentCharacter = ModelDb.Character<Code.Character.HoolheyakMod>();

        UpdateUI();
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
    }

    private void UpdateUI()
    {
        if (_currentCharacter == null || _nameLabel == null || _modelPlaceholder == null)
            return;

        int skinIndex = HoolheyakSkinState.Normalize(HoolheyakConfig.CurrentSkinIndex);

        string currentSkinKey = $"HOOLHEYAKMOD-HOOLHEYAK.skin{skinIndex}";
        _nameLabel.Text = LocString.Exists("characters", currentSkinKey)
            ? new LocString("characters", currentSkinKey).GetFormattedText()
            : $"Skin {skinIndex}";

        foreach (Node child in _modelPlaceholder.GetChildren())
        {
            _modelPlaceholder.RemoveChild(child);
            child.QueueFree();
        }

        string modelPath = _currentCharacter.CustomVisualPath;
        if (Godot.FileAccess.FileExists(modelPath))
        {
            PackedScene skinScene = GD.Load<PackedScene>(modelPath);
            if (skinScene != null)
            {
                Node2D skinInstance = skinScene.Instantiate<Node2D>();
                _modelPlaceholder.AddChild(skinInstance);
                skinInstance.Position = Vector2.Zero;
                PlayIdleAnimation(skinInstance);
            }
        }
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

            if (current is AnimationPlayer animPlayer)
            {
                if (animPlayer.HasAnimation("idle_loop"))
                {
                    animPlayer.Play("idle_loop");
                    return;
                }
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
            {
                queue.Enqueue(child);
            }
        }
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
