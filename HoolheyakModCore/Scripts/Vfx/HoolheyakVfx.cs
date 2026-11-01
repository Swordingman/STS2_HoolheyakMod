using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.TestSupport;

namespace HoolheyakMod.Scripts.Vfx;

internal static class HoolheyakVfx
{
    private const string SceneRoot = "res://HoolheyakMod/scenes/vfx/heyak/";
    private static readonly Dictionary<string, PackedScene> Scenes = new();

    public static void PlayStorm(Creature target) =>
        PlayAtCenter(target, "HoolheyakStormVfx", 1.3f, 0.85f, coalesce: true, verticalOffset: -130f);

    public static void PlayImpact(Creature target) =>
        PlayAtCenter(target, "HoolheyakHitVfx", 1.3f, 0.28f);

    public static void PlayCyclone(Creature target) =>
        PlayAtCenter(target, "HoolheyakCycloneVfx", 1.8f, 1.05f);

    public static async Task PlayProjectile(Creature source, Creature target)
    {
        if (TestMode.IsOn || source.IsDead || target.IsDead)
            return;

        var sourceNode = source.GetCreatureNode();
        var targetNode = target.GetCreatureNode();
        Control? container = target.GetVfxContainer();
        if (sourceNode == null || targetNode == null || container == null || container.IsQueuedForDeletion())
            return;

        Node2D? effect = Create("HoolheyakProjectileVfx");
        if (effect == null)
            return;

        const float flightTime = 0.22f;
        Transform2D toLocal = container.GetGlobalTransform().AffineInverse();
        Vector2 start = toLocal * sourceNode.VfxSpawnPosition;
        Vector2 destination = toLocal * targetNode.VfxSpawnPosition;
        effect.Scale = Vector2.One * 1.3f;
        effect.Call("configure_projectile", start, destination, flightTime);
        container.AddChildSafely(effect);
        // Called by AttackCommand.AfterAttackerAnim; damage and hit FX follow arrival.
        await Cmd.Wait(flightTime);
    }

    private static void PlayAtCenter(Creature target, string scene, float scale, float duration, bool coalesce = false, float verticalOffset = 0f)
    {
        if (TestMode.IsOn || target.IsDead)
            return;

        var targetNode = target.GetCreatureNode();
        Control? container = target.GetVfxContainer();
        if (targetNode == null || container == null || container.IsQueuedForDeletion())
            return;

        string name = $"{scene}_{targetNode.GetInstanceId()}";
        if (coalesce && container.GetNodeOrNull<Node2D>(name) is { } existing && !existing.IsQueuedForDeletion())
            return;

        Node2D? effect = Create(scene);
        if (effect == null)
            return;

        if (coalesce)
            effect.Name = name;
        effect.Scale = Vector2.One * scale;
        effect.Set("duration", duration);
        container.AddChildSafely(effect);
        // The storm's ground ring sits below its scene origin; raise the enlarged
        // storm without moving centered impacts or the other effect scenes.
        effect.GlobalPosition = targetNode.VfxSpawnPosition + new Vector2(0f, verticalOffset);
    }

    private static Node2D? Create(string scene)
    {
        if (!Scenes.TryGetValue(scene, out PackedScene? packed))
        {
            // VfxCmd's SceneHelper prefixes res://scenes/. Load our full paths directly.
            packed = ResourceLoader.Load<PackedScene>($"{SceneRoot}{scene}.tscn");
            if (packed == null)
                return null;
            Scenes.Add(scene, packed);
        }

        return packed.Instantiate<Node2D>();
    }
}
