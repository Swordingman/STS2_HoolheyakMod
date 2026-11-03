using System;
using System.Reflection;
using BaseLib.Utils.NodeFactories;
using Godot;
using HarmonyLib;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Models.Events;

namespace HoolheyakMod.Code.Patches;

internal static class PlayerSkinSceneHelper
{
    public static int GetSkin(Player player)
    {
        // 使用 Player 重载：本地玩家回退到已保存的皮肤配置，联机玩家使用网络同步的皮肤。
        return HoolheyakSkinState.GetSkin(player);
    }

    public static NCreatureVisuals CreateCombatVisuals(Player player)
    {
        if (player.Character is not Code.Character.HoolheyakMod)
            return player.Character.CreateVisuals();

        int skinIndex = GetSkin(player);
        string path = Code.Character.HoolheyakMod.GetVisualPath(skinIndex);

        GD.Print($"[HoolheyakMod Skin] 创建战斗模型：NetId={player.NetId}, Skin={skinIndex}, Path={path}");

        return NodeFactory<NCreatureVisuals>.CreateFromScene(path);
    }

    public static void PlayMerchantLoop(Node root)
    {
        Node? spineSprite = FindSpineSprite(root);
        if (spineSprite == null)
        {
            GD.PushWarning("[HoolheyakMod Skin] 商店模型中没有找到 SpineSprite。");
            return;
        }

        try
        {
            var sprite = new MegaSprite(spineSprite);
            var animationState = sprite.GetAnimationState();

#if STS2_BETA
            animationState.SetAnimation("relaxed_loop", true);
#else
            MegaTrackEntry? entry = animationState.SetAnimation("relaxed_loop", true);
            if (entry != null)
            {
                float animationEnd = entry.GetAnimationEnd();
                if (animationEnd > 0f)
                {
                    entry.SetTrackTime(animationEnd * Rng.Chaotic.NextFloat());
                }
            }
#endif
        }
        catch (Exception e)
        {
            GD.PushWarning($"[HoolheyakMod Skin] 播放真实商店动画失败：{e.Message}");
        }
    }

    private static Node? FindSpineSprite(Node root)
    {
        Queue<Node> nodes = new();
        nodes.Enqueue(root);

        while (nodes.Count > 0)
        {
            Node current = nodes.Dequeue();
            if (current.GetClass() == "SpineSprite")
                return current;

            foreach (Node child in current.GetChildren())
                nodes.Enqueue(child);
        }

        return null;
    }
}

[HarmonyPatch(typeof(Creature), nameof(Creature.CreateVisuals))]
internal static class CombatPlayerSkinPatch
{
    [HarmonyPrefix]
    private static bool Prefix(Creature __instance, ref NCreatureVisuals? __result)
    {
        Player? player = __instance.Player;

        if (player?.Character is not Code.Character.HoolheyakMod)
            return true;

        __result = PlayerSkinSceneHelper.CreateCombatVisuals(player);
        return false;
    }
}

[HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter.Create))]
internal static class RestSitePlayerSkinPatch
{
    private static readonly MethodInfo? PlayerSetter =
        AccessTools.PropertySetter(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter.Player));

    private static readonly FieldInfo? CharacterIndexField =
        AccessTools.Field(typeof(NRestSiteCharacter), "_characterIndex");

    [HarmonyPrefix]
    private static bool Prefix(Player player, int characterIndex, ref NRestSiteCharacter? __result)
    {
        if (player.Character is not Code.Character.HoolheyakMod)
            return true;

        if (PlayerSetter == null || CharacterIndexField == null)
        {
            GD.PushWarning("[HoolheyakMod Skin] 无法访问篝火角色内部字段，使用原版加载逻辑。");
            return true;
        }

        int skinIndex = PlayerSkinSceneHelper.GetSkin(player);
        string path = Code.Character.HoolheyakMod.GetRestSiteAnimPath(skinIndex);

        NRestSiteCharacter character = NodeFactory<NRestSiteCharacter>.CreateFromScene(path);

        PlayerSetter.Invoke(character, new object?[] { player });
        CharacterIndexField.SetValue(character, characterIndex);

        __result = character;

        GD.Print($"[HoolheyakMod Skin] 篝火模型：NetId={player.NetId}, Skin={skinIndex}, Path={path}");

        return false;
    }
}

internal static class FakeMerchantAnimationHelper
{
    public static void Play(NCreatureVisuals visuals)
    {
        MegaTrackEntry? entry = TrySetAnimation(visuals, "relaxed_loop");
        if (entry == null) entry = TrySetAnimation(visuals, "idle_loop");
        if (entry == null) return;

        entry.SetLoop(true);
        entry.SetTimeScale(Rng.Chaotic.NextFloat(0.9f, 1.1f));

        float animationEnd = entry.GetAnimationEnd();
        if (animationEnd <= 0f) return;

        float trackTime = (animationEnd + Rng.Chaotic.NextFloat(-0.5f, 0.5f)) % animationEnd;
        if (trackTime < 0f) trackTime += animationEnd;
        entry.SetTrackTime(trackTime);
    }

    private static MegaTrackEntry? TrySetAnimation(NCreatureVisuals visuals, string animationName)
    {
        try
        {
#if STS2_BETA
            visuals.SpineAnimation.SetAnimation(animationName);
            return null;
#else
            return visuals.SpineAnimation.SetAnimation(animationName);
#endif
        }
        catch (Exception e)
        {
            GD.PushWarning($"[HoolheyakMod] 设置假商人动画失败 '{animationName}'：{e.Message}");
            return null;
        }
    }
}

// Let the game (or another character mod) build the shared player list first.
// Only replace our own visuals afterwards; never take over other characters.
[HarmonyPatch(typeof(NMerchantRoom), "AfterRoomIsLoaded")]
internal static class MerchantPlayerSkinPatch
{
    [HarmonyPostfix]
    private static void Postfix(List<Player> ____players, Control ____characterContainer,
        List<NMerchantCharacter> ____playerVisuals)
    {
        for (int i = 0; i < ____players.Count && i < ____playerVisuals.Count; i++)
        {
            Player player = ____players[i];
            if (player.Character is not Code.Character.HoolheyakMod)
                continue;

            int skinIndex = PlayerSkinSceneHelper.GetSkin(player);
            string path = Code.Character.HoolheyakMod.GetMerchantAnimPath(skinIndex);
            NMerchantCharacter oldCharacter = ____playerVisuals[i];
            if (oldCharacter.SceneFilePath == path)
                continue;

            // All three scenes carry a concrete NMerchantCharacter script, so
            // direct PackedScene instantiation also works in other mods' loaders.
            var scene = ResourceLoader.Load<PackedScene>(path);
            NMerchantCharacter character = scene.Instantiate<NMerchantCharacter>();
            character.Transform = oldCharacter.Transform;
            character.Modulate = oldCharacter.Modulate;
            character.ZIndex = oldCharacter.ZIndex;
            int childIndex = oldCharacter.GetIndex();

            ____characterContainer.AddChildSafely(character);
            ____characterContainer.MoveChild(character, childIndex);
            ____characterContainer.RemoveChild(oldCharacter);
            oldCharacter.QueueFree();
            ____playerVisuals[i] = character;

            GD.Print($"[HoolheyakMod Skin] 商店皮肤更新：NetId={player.NetId}, Skin={skinIndex}, Path={path}");
        }
    }
}

[HarmonyPatch(typeof(NFakeMerchant), "AfterRoomIsLoaded")]
internal static class FakeMerchantPlayerSkinPatch
{
    private static readonly MethodInfo? ShowWelcomeDialogueMethod =
        AccessTools.Method(typeof(NFakeMerchant), "ShowWelcomeDialogue");

    [HarmonyPrefix]
    private static bool Prefix(NFakeMerchant __instance, List<Player> ____players, Control ____characterContainer, FakeMerchant ____event)
    {
        Player? localPlayer = LocalContext.GetMe(____players);
        if (localPlayer == null)
        {
            GD.PushWarning("[HoolheyakMod Skin] 假商人中未找到本地玩家，使用原版加载逻辑。");
            return true;
        }

        ____players.Remove(localPlayer);
        ____players.Insert(0, localPlayer);

        int rowCount = Mathf.CeilToInt(Mathf.Sqrt(____players.Count));

        for (int row = 0; row < rowCount; row++)
        {
            float x = -75f * row;
            for (int column = 0; column < rowCount; column++)
            {
                int playerIndex = row * rowCount + column;
                if (playerIndex >= ____players.Count) break;

                Player player = ____players[playerIndex];
                NCreatureVisuals visuals = PlayerSkinSceneHelper.CreateCombatVisuals(player);

                ____characterContainer.AddChildSafely(visuals);
                FakeMerchantAnimationHelper.Play(visuals);
                ____characterContainer.MoveChild(visuals, 0);
                visuals.Position = new Vector2(x, -50f * row);

                if (row > 0)
                {
                    visuals.Modulate = new Color(0.5f, 0.5f, 0.5f);
                }

                x -= visuals.Bounds.Size.X * 0.5f + 25f;
            }
        }

        if (!____event.StartedFight)
            StartWelcomeDialogue(__instance);

        return false;
    }

    private static void StartWelcomeDialogue(NFakeMerchant instance)
    {
        if (ShowWelcomeDialogueMethod == null)
        {
            GD.PushWarning("[HoolheyakMod Skin] 无法找到假商人欢迎对白方法。");
            return;
        }

        try
        {
            if (ShowWelcomeDialogueMethod.Invoke(instance, null) is Task dialogueTask)
            {
                TaskHelper.RunSafely(dialogueTask);
            }
        }
        catch (Exception e)
        {
            GD.PushWarning($"[HoolheyakMod Skin] 启动假商人对白失败：{e.Message}");
        }
    }
}

[HarmonyPatch(typeof(NFakeMerchant), "StartCharacterAnimation")]
internal static class FakeMerchantAnimationPatch
{
    [HarmonyPrefix]
    private static bool Prefix(NCreatureVisuals visuals)
    {
        if (visuals != null)
            FakeMerchantAnimationHelper.Play(visuals);

        return false;
    }
}

[HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter._Ready))]
internal static class RestSiteAnimationPatch
{
    [HarmonyPostfix]
    private static void Postfix(NRestSiteCharacter __instance)
    {
        if (__instance.Player?.Character is not Code.Character.HoolheyakMod)
            return;

        foreach (Node child in __instance.GetChildren())
        {
            if (child is not Node2D spineNode || spineNode.GetClass() != MegaSprite.spineClassName)
                continue;

            var sprite = new MegaSprite(spineNode);
            if (sprite.HasAnimation("Sit"))
                sprite.GetAnimationState().SetAnimation("Sit", true);
        }
    }
}
