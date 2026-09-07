using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;

namespace HoolheyakMod.Scripts.Utils;

public static class HoolheyakSkinState
{
    private const int SkinCount = 3;
    private static readonly Dictionary<ulong, int> SkinIndices = new();

    public static int Normalize(int index)
    {
        index %= SkinCount;
        if (index < 0) index += SkinCount;
        return index;
    }

    public static void SetSkin(ulong playerNetId, int skinIndex)
    {
        SkinIndices[playerNetId] = Normalize(skinIndex);
    }

    public static int GetSkin(ulong playerNetId)
    {
        return SkinIndices.TryGetValue(playerNetId, out int skinIndex)
            ? Normalize(skinIndex)
            : 0;
    }

    public static int GetSkin(Player? player)
    {
        if (player == null)
            return Normalize(HoolheyakConfig.CurrentSkinIndex);

        return SkinIndices.TryGetValue(player.NetId, out int skinIndex)
            ? Normalize(skinIndex)
            : Normalize(HoolheyakConfig.CurrentSkinIndex);
    }

    public static void RemovePlayer(ulong playerNetId)
    {
        SkinIndices.Remove(playerNetId);
    }

    public static void Clear()
    {
        SkinIndices.Clear();
    }
}
