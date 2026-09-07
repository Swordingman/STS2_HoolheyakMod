using System;
using System.Collections.Generic;
using Godot;
using HoolheyakMod.Code.Powers;
using HoolheyakMod.Scripts.Phases;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace HoolheyakMod.Scripts.Phases;

public partial class NPhaseAuroraVfx : Node2D
{
    private const float FadeInSpeed = 0.50f;
    private const float FadeOutSpeed = 1.50f;
    private const float ParticleSpawnEventsPerSecond = 24f;
    private const int MaxParticles = 160;

    private readonly List<AuroraBeam> _beams = new();
    private readonly List<StarParticle> _particles = new();
    private readonly RandomNumberGenerator _rng = new();

    private ColorRect _darkness = null!;
    private Node2D _auroraLayer = null!;
    private Node2D _particleLayer = null!;
    private CanvasItemMaterial _additiveMaterial = null!;

    private Creature? _owner;
    private PhaseType _phase;
    private Color _mainColor = Colors.White;
    private float _alpha;
    private float _maxAlpha = 0.5f;
    private float _particleSpawnAccumulator;
    private bool _isFadingOut;
    private bool _finishedRaised;

    public Creature? OwnerCreature => _owner;
    public bool IsFadingOut => _isFadingOut;

    public event Action<NPhaseAuroraVfx>? Finished;

    public void Initialize(Creature owner, PhaseType phase, Color mainColor, float maxAlpha)
    {
        _owner = owner;
        _phase = phase;
        _mainColor = mainColor;
        _maxAlpha = Math.Max(0.01f, maxAlpha);
    }

    public override void _Ready()
    {
        _darkness = GetNode<ColorRect>("%Darkness");
        _auroraLayer = GetNode<Node2D>("%AuroraLayer");
        _particleLayer = GetNode<Node2D>("%ParticleLayer");

        _rng.Randomize();
        _additiveMaterial = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };

        Vector2 viewportSize = GetViewportRect().Size;
        _darkness.Position = Vector2.Zero;
        _darkness.Size = viewportSize;

        CreateAuroraBeams(viewportSize);

        if (_owner == null)
            ForceFadeOut();
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        bool phaseIsActive = !_isFadingOut && HasExpectedPhase();

        if (phaseIsActive)
        {
            _alpha = Math.Min(_maxAlpha, _alpha + dt * FadeInSpeed);

            if (_alpha > _maxAlpha * 0.30f)
                SpawnParticles(dt);
        }
        else
        {
            _isFadingOut = true;
            _alpha = Math.Max(0f, _alpha - dt * FadeOutSpeed);
        }

        UpdateDarkness();
        UpdateAuroraBeams(dt);
        UpdateParticles(dt);

        if (_isFadingOut && _alpha <= 0f && _particles.Count == 0)
            Finish();
    }

    public void ForceFadeOut()
    {
        _isFadingOut = true;
    }

    private bool HasExpectedPhase()
    {
        if (_owner?.CombatState == null)
            return false;

        return _phase switch
        {
            PhaseType.Conjunction => _owner.GetPower<ConjunctionPower>() != null,
            PhaseType.Quincunx => _owner.GetPower<QuincunxPower>() != null,
            PhaseType.Sextile => _owner.GetPower<SextilePower>() != null,
            PhaseType.Trine => _owner.GetPower<TrinePower>() != null,
            PhaseType.Square => _owner.GetPower<SquarePower>() != null,
            PhaseType.Opposition => _owner.GetPower<OppositionPower>() != null,
            _ => false
        };
    }

    private void CreateAuroraBeams(Vector2 viewportSize)
    {
        int layerCount = _rng.RandiRange(8, 12);
        float lightHeight = viewportSize.Y * 1.25f;

        for (int i = 0; i < layerCount; i++)
        {
            float width = _rng.RandfRange(viewportSize.X * 0.08f, viewportSize.X * 0.25f);
            float topWidth = width * _rng.RandfRange(0.10f, 0.25f);
            float xOffset = _rng.RandfRange(-viewportSize.X * 0.032f, viewportSize.X * 0.032f);
            float baseAngle = _rng.RandfRange(-55f, 55f);
            float timer = _rng.RandfRange(0f, 100f);
            float speed = _rng.RandfRange(0.2f, 0.6f);

            Color beamColor = GetBeamColor(i);
            beamColor.A = 0f;

            var beamNode = new Polygon2D
            {
                Polygon =
                [
                    new Vector2(-topWidth * 0.5f, 0f),
                    new Vector2(topWidth * 0.5f, 0f),
                    new Vector2(width * 0.5f, lightHeight),
                    new Vector2(-width * 0.5f, lightHeight)
                ],
                Position = new Vector2(viewportSize.X * 0.5f + xOffset, -viewportSize.Y * 0.18f),
                Rotation = Mathf.DegToRad(baseAngle),
                Color = beamColor,
                Material = _additiveMaterial
            };

            _auroraLayer.AddChild(beamNode);
            _beams.Add(new AuroraBeam(beamNode, beamColor, timer, speed, baseAngle));
        }
    }

    private Color GetBeamColor(int index)
    {
        Color color = _mainColor;

        if (index % 3 == 1)
            color.R = Math.Min(1f, color.R + 0.15f);
        else if (index % 3 == 2)
            color.B = Math.Min(1f, color.B + 0.15f);

        return color;
    }

    private void UpdateDarkness()
    {
        float masterAlpha = _alpha / _maxAlpha;
        float darknessAlpha = _isFadingOut ? 0f : 0.40f * masterAlpha;
        _darkness.Color = new Color(0f, 0f, 0f, darknessAlpha);
    }

    private void UpdateAuroraBeams(float dt)
    {
        foreach (AuroraBeam beam in _beams)
        {
            beam.Timer += dt * beam.Speed;

            float layerAlpha = _alpha + Mathf.Sin(beam.Timer) * 0.15f;
            layerAlpha = Mathf.Clamp(layerAlpha, 0f, 1f);

            Color color = beam.BaseColor;
            color.A = layerAlpha * 0.12f;

            beam.Node.Color = color;
            beam.Node.Rotation = Mathf.DegToRad(beam.BaseAngle + Mathf.Sin(beam.Timer * 0.4f) * 12f);
        }
    }

    private void SpawnParticles(float dt)
    {
        if (_particles.Count >= MaxParticles)
            return;

        _particleSpawnAccumulator += dt * ParticleSpawnEventsPerSecond;

        while (_particleSpawnAccumulator >= 1f && _particles.Count < MaxParticles)
        {
            _particleSpawnAccumulator -= 1f;
            int spawnCount = _rng.RandiRange(1, 2);

            for (int i = 0; i < spawnCount && _particles.Count < MaxParticles; i++)
                CreateParticle();
        }
    }

    private void CreateParticle()
    {
        Vector2 viewportSize = GetViewportRect().Size;
        float scale = viewportSize.Y / 1080f;
        float life = _rng.RandfRange(2f, 6f);
        float length = _rng.RandfRange(6f, 20f) * scale;

        Color particleColor = new Color(
            Math.Min(1f, _mainColor.R + 0.30f),
            Math.Min(1f, _mainColor.G + 0.30f),
            Math.Min(1f, _mainColor.B + 0.30f),
            0f);

        var line = new Line2D
        {
            Width = Math.Max(0.75f, 1.1f * scale),
            DefaultColor = particleColor,
            Material = _additiveMaterial,
            Antialiased = true,
            Position = new Vector2(
                viewportSize.X * 0.5f + _rng.RandfRange(-viewportSize.X * 0.31f, viewportSize.X * 0.31f),
                _rng.RandfRange(-100f, -10f) * scale),
            Rotation = _rng.RandfRange(-0.35f, 0.35f)
        };

        line.Points = [new Vector2(0f, -length * 0.5f), new Vector2(0f, length * 0.5f)];
        _particleLayer.AddChild(line);

        _particles.Add(new StarParticle(
            line,
            particleColor,
            new Vector2(_rng.RandfRange(-30f, 30f), _rng.RandfRange(60f, 180f)) * scale,
            _rng.RandfRange(-1.5f, 1.5f),
            life,
            _rng.RandfRange(0f, 10f),
            _rng.RandfRange(3f, 8f)));
    }

    private void UpdateParticles(float dt)
    {
        float masterAlpha = _alpha / _maxAlpha;

        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            StarParticle particle = _particles[i];
            particle.Life -= dt;

            if (particle.Life <= 0f)
            {
                particle.Node.QueueFree();
                _particles.RemoveAt(i);
                continue;
            }

            particle.Node.Position += particle.Velocity * dt;
            particle.Node.Rotation += particle.RotationSpeed * dt;

            float alpha = particle.Life / particle.MaxLife;
            float age = particle.MaxLife - particle.Life;

            if (age < 0.5f)
                alpha = age / 0.5f;

            float twinkle = (Mathf.Sin(particle.Life * particle.TwinkleSpeed + particle.TwinkleOffset) + 1f) * 0.5f;
            alpha *= 0.30f + 0.70f * twinkle;

            Color color = particle.BaseColor;
            color.A = alpha * masterAlpha;
            particle.Node.DefaultColor = color;
        }
    }

    private void Finish()
    {
        if (_finishedRaised)
            return;

        _finishedRaised = true;
        Finished?.Invoke(this);
        QueueFree();
    }

    private sealed class AuroraBeam
    {
        public Polygon2D Node { get; }
        public Color BaseColor { get; }
        public float Speed { get; }
        public float BaseAngle { get; }
        public float Timer { get; set; }

        public AuroraBeam(Polygon2D node, Color baseColor, float timer, float speed, float baseAngle)
        {
            Node = node;
            BaseColor = baseColor;
            Timer = timer;
            Speed = speed;
            BaseAngle = baseAngle;
        }
    }

    private sealed class StarParticle
    {
        public Line2D Node { get; }
        public Color BaseColor { get; }
        public Vector2 Velocity { get; }
        public float RotationSpeed { get; }
        public float MaxLife { get; }
        public float TwinkleOffset { get; }
        public float TwinkleSpeed { get; }
        public float Life { get; set; }

        public StarParticle(Line2D node, Color baseColor, Vector2 velocity, float rotationSpeed, float life,
            float twinkleOffset, float twinkleSpeed)
        {
            Node = node;
            BaseColor = baseColor;
            Velocity = velocity;
            RotationSpeed = rotationSpeed;
            Life = life;
            MaxLife = life;
            TwinkleOffset = twinkleOffset;
            TwinkleSpeed = twinkleSpeed;
        }
    }
}
