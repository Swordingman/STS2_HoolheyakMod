using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BaseLib.Patches.Saves;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace HoolheyakMod.Scripts.Utils;

public static class HoolheyakSkinState
{
    private const int SkinCount = 3;
    internal const string SaveKey = "HoolheyakMod.PlayerSkin";
    private static readonly Dictionary<ulong, int> SkinIndices = new();
    private sealed class Selection { public int Skin = -1; }
    private static readonly ConditionalWeakTable<Player, Selection> PlayerSkins = new();
    private static bool _saveRegistered;

    public static void RegisterSave()
    {
        if (_saveRegistered) return;
        ExtendedSaveHandlers<Player, SerializablePlayer>.RegisterSave<int>(SaveKey,
            GetSavedSkin, RestoreSkin,
            (skin, writer) => writer.WriteInt(skin, 32), reader => reader.ReadInt(32));
        _saveRegistered = true;
    }

    public static int Normalize(int index) => ((index % SkinCount) + SkinCount) % SkinCount;
    public static void SetSkin(ulong playerNetId, int skinIndex) => SkinIndices[playerNetId] = Normalize(skinIndex);
    internal static bool TryGetSkin(ulong playerNetId, out int skinIndex) => SkinIndices.TryGetValue(playerNetId, out skinIndex);
    public static int GetSkin(ulong playerNetId) => TryGetSkin(playerNetId, out int skin) ? skin : 0;

    private static int GetSavedSkin(Player player)
    {
        var selection = PlayerSkins.GetOrCreateValue(player);
        if (selection.Skin >= 0) return selection.Skin;
        if (TryGetSkin(player.NetId, out int skin)) return selection.Skin = skin;
        if (LocalContext.IsMe(player))
            return selection.Skin = Normalize(HoolheyakConfig.CurrentSkinIndex);
        // Unknown is distinct from an explicitly selected skin 0.
        return -1;
    }

    private static void RestoreSkin(Player player, int skin)
    {
        if (skin < 0) return;
        PlayerSkins.GetOrCreateValue(player).Skin = Normalize(skin);
        SetSkin(player.NetId, skin);
    }

    public static int GetSkin(Player? player) => player == null
        ? Normalize(HoolheyakConfig.CurrentSkinIndex) : System.Math.Max(0, GetSavedSkin(player));

    internal static bool TryRestore(SerializablePlayer player)
    {
        var data = ExtendedSaveHandlers<Player, SerializablePlayer>.ExtendedData[player].DictForType<int>();
        if (!data.TryGetValue(SaveKey, out int skin) || skin < 0) return false;
        SetSkin(player.NetId, skin);
        return true;
    }

    internal static void Store(SerializablePlayer player, int skin) =>
        ExtendedSaveHandlers<Player, SerializablePlayer>.ExtendedData[player].DictForType<int>()[SaveKey] = Normalize(skin);

    public static void RemovePlayer(ulong playerNetId) => SkinIndices.Remove(playerNetId);
    public static void Clear() => SkinIndices.Clear();
}
