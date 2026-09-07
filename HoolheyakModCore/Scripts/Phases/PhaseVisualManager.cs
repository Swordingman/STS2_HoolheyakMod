using System;
using System.Collections.Generic;
using Godot;
using HoolheyakMod.Scripts.Phases;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HoolheyakMod.Scripts.Phases;

public static class PhaseVisualManager
{
    public const string ScenePath = "res://HoolheyakMod/scenes/PhaseAuroraVfx.tscn";

    private static readonly Dictionary<Creature, NPhaseAuroraVfx> ActiveVfx = new();
    private static PackedScene? _scene;

    public static NPhaseAuroraVfx? Play(Creature owner, PhaseType phase)
    {
        Node? container = NCombatRoom.Instance?.CombatVfxContainer;
        if (container == null)
            return null;

        Stop(owner);

        PhaseVisualStyle style = GetStyle(phase);
        NPhaseAuroraVfx vfx = Scene.Instantiate<NPhaseAuroraVfx>();

        vfx.Initialize(owner, phase, style.MainColor, style.MaxAlpha);
        vfx.Finished += OnVfxFinished;

        container.AddChildSafely(vfx);
        ActiveVfx[owner] = vfx;

        return vfx;
    }

    public static void Stop(Creature owner)
    {
        if (!ActiveVfx.TryGetValue(owner, out NPhaseAuroraVfx? vfx))
            return;

        ActiveVfx.Remove(owner);

        if (GodotObject.IsInstanceValid(vfx))
            vfx.ForceFadeOut();
    }

    public static void StopAll()
    {
        var active = new List<NPhaseAuroraVfx>(ActiveVfx.Values);
        ActiveVfx.Clear();

        foreach (NPhaseAuroraVfx vfx in active)
        {
            if (GodotObject.IsInstanceValid(vfx))
                vfx.ForceFadeOut();
        }
    }

    private static PackedScene Scene => _scene ??= ResourceLoader.Load<PackedScene>(ScenePath)
        ?? throw new InvalidOperationException($"Could not load phase VFX scene: {ScenePath}");

    private static void OnVfxFinished(NPhaseAuroraVfx vfx)
    {
        Creature? owner = vfx.OwnerCreature;
        if (owner == null)
            return;

        if (ActiveVfx.TryGetValue(owner, out NPhaseAuroraVfx? current) && ReferenceEquals(current, vfx))
            ActiveVfx.Remove(owner);
    }

    private static PhaseVisualStyle GetStyle(PhaseType phase)
    {
        return phase switch
        {
            PhaseType.Conjunction => new PhaseVisualStyle(new Color(1.00f, 0.85f, 0.20f), 0.65f),
            PhaseType.Sextile => new PhaseVisualStyle(new Color(0.20f, 0.80f, 0.80f), 0.45f),
            PhaseType.Square => new PhaseVisualStyle(new Color(1.00f, 0.50f, 0.00f), 0.60f),
            PhaseType.Trine => new PhaseVisualStyle(new Color(0.20f, 0.90f, 0.30f), 0.50f),
            PhaseType.Quincunx => new PhaseVisualStyle(new Color(0.60f, 0.10f, 0.80f), 0.50f),
            PhaseType.Opposition => new PhaseVisualStyle(new Color(0.90f, 0.05f, 0.05f), 0.70f),
            _ => new PhaseVisualStyle(Colors.White, 0.50f)
        };
    }

    private readonly record struct PhaseVisualStyle(Color MainColor, float MaxAlpha);
}
