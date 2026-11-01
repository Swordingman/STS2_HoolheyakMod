using System;
using System.Collections.Generic;
using Godot;
using HoolheyakMod.Scripts.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace HoolheyakMod.Scripts;

/// <summary>卡牌边缘的柔边烟团；不消耗游戏随机数，不修改牌或原生高亮。</summary>
public partial class NSloughSmokeVfx : Node2D
{
    public const string NodeName = "HoolheyakSloughSmoke";
    private readonly Random _visualRandom = new();
    private readonly List<Puff> _puffs = [];
    private static Texture2D? _smokeTexture;
    private NCard? _card;
    private float _spawn;
    private float _intensity;
    private Rect2 _bounds;

    public override void _Ready()
    {
        _card = GetParentOrNull<NCard>();
        ZIndex = -1;
        TextureFilter = TextureFilterEnum.Linear;
        ProcessMode = ProcessModeEnum.Pausable;
        _smokeTexture ??= CreateSmokeTexture();
    }

    public override void _Process(double delta)
    {
        if (_card == null || !GodotObject.IsInstanceValid(_card)) { QueueFree(); return; }
        if (!_card.IsVisibleInTree())
        {
            _puffs.Clear();
            _intensity = _spawn = 0f;
            Visible = false;
            return;
        }
        float dt = Math.Min((float)delta, 0.05f);
        bool active = _card.Model != null &&
            CardGlowRules.CanTriggerSlough(_card.Model);
        _intensity = Mathf.MoveToward(_intensity, active ? 1f : 0f, dt * 5f);

        _bounds = GetCardBounds(_card).Grow(1f);
        if (active)
        {
            _spawn += dt * 26f;
            while (_spawn >= 1f && _puffs.Count < 36)
            {
                _spawn -= 1f;
                Vector2 point = OutlinePoint(_bounds, (float)_visualRandom.NextDouble());
                var outward = (point - _bounds.GetCenter()).Normalized();
                _puffs.Add(new Puff(point, outward * Range(10f, 22f) + new Vector2(0f, -Range(16f, 32f)),
                    Range(16f, 26f), Range(0.8f, 1.35f), Range(0f, Mathf.Tau)));
            }
            _spawn = Math.Min(_spawn, 1f);
        }
        else _spawn = 0f;

        for (int i = _puffs.Count - 1; i >= 0; i--)
        {
            var p = _puffs[i];
            p.Age += dt;
            p.Position += p.Velocity * dt;
            if (p.Age >= p.Lifetime) _puffs.RemoveAt(i);
            else _puffs[i] = p;
        }
        Visible = _intensity > 0.01f && _puffs.Count > 0;
        if (Visible) QueueRedraw();
    }

    private static Rect2 GetCardBounds(NCard card)
    {
        // Highlight includes the native glow's padding. Using it makes the effect
        // a much larger rectangle; this node already inherits the card's scale.
        return new Rect2(Vector2.Zero, card.Size.X > 0f && card.Size.Y > 0f
            ? card.Size : new Vector2(250f, 351f));
    }

    internal static Vector2 OutlinePoint(Rect2 bounds, float progress)
    {
        progress -= MathF.Floor(progress);
        float radius = Math.Min(16f, Math.Min(bounds.Size.X, bounds.Size.Y) * 0.08f);
        float horizontal = bounds.Size.X - radius * 2f;
        float vertical = bounds.Size.Y - radius * 2f;
        float arc = radius * Mathf.Pi * 0.5f;
        float distance = progress * (2f * horizontal + 2f * vertical + 4f * arc);
        var origin = bounds.Position;
        Vector2 Corner(Vector2 center, float angle) => origin + center +
            new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
        // Uniform distance sampling avoids concentrating the smoke at the corners.
        if (distance < horizontal) return origin + new Vector2(radius + distance, 0f);
        distance -= horizontal;
        if (distance < arc) return Corner(new(bounds.Size.X - radius, radius), -Mathf.Pi * 0.5f + distance / radius);
        distance -= arc;
        if (distance < vertical) return origin + new Vector2(bounds.Size.X, radius + distance);
        distance -= vertical;
        if (distance < arc) return Corner(new(bounds.Size.X - radius, bounds.Size.Y - radius), distance / radius);
        distance -= arc;
        if (distance < horizontal) return origin + new Vector2(bounds.Size.X - radius - distance, bounds.Size.Y);
        distance -= horizontal;
        if (distance < arc) return Corner(new(radius, bounds.Size.Y - radius), Mathf.Pi * 0.5f + distance / radius);
        distance -= arc;
        if (distance < vertical) return origin + new Vector2(0f, bounds.Size.Y - radius - distance);
        distance -= vertical;
        return Corner(new(radius, radius), Mathf.Pi + distance / radius);
    }

    public override void _Draw()
    {
        if (_smokeTexture == null) return;
        foreach (var p in _puffs)
        {
            float life = p.Age / p.Lifetime;
            float alpha = MathF.Sin(life * MathF.PI) * 0.65f * _intensity;
            var pos = p.Position + new Vector2(MathF.Sin(p.Phase + life * 5f) * 5f, 0f);
            float radius = p.Radius * (0.65f + life * 0.85f);
            var size = new Vector2(radius * 2f, radius * 2.5f);
            DrawTextureRect(_smokeTexture, new Rect2(pos - size * 0.5f, size), false,
                new Color(0.065f, 0.05f, 0.08f, alpha));
        }
    }

    private static Texture2D CreateSmokeTexture()
    {
        const int size = 64;
        var image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = (x + 0.5f - size * 0.5f) / (size * 0.5f);
            float dy = (y + 0.5f - size * 0.5f) / (size * 0.5f);
            float edge = Math.Max(0f, 1f - dx * dx - dy * dy);
            float cloud = 0.65f + 0.18f * MathF.Sin(x * 0.41f + MathF.Sin(y * 0.3f)) +
                0.17f * MathF.Cos(y * 0.37f - x * 0.23f);
            image.SetPixel(x, y, new Color(1f, 1f, 1f, edge * edge * cloud));
        }
        var texture = ImageTexture.CreateFromImage(image);
        image.Dispose();
        return texture;
    }

    private float Range(float min, float max) => min + (float)_visualRandom.NextDouble() * (max - min);
    private struct Puff(Vector2 position, Vector2 velocity, float radius, float lifetime, float phase)
    {
        public Vector2 Position = position;
        public Vector2 Velocity = velocity;
        public float Radius = radius;
        public float Lifetime = lifetime;
        public float Phase = phase;
        public float Age;
    }
}
