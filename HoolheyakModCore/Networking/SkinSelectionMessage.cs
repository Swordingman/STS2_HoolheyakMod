using BaseLib.Abstracts;
using Godot;
using HoolheyakMod.Scripts.Utils;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace HoolheyakMod.Code.Networking;

public sealed class SkinSelectionMessage : ICustomMessage
{
    public int SkinIndex { get; private set; }

    public bool ShouldBroadcast => true;
    public bool ShouldBuffer => false;

    public SkinSelectionMessage() { }

    public SkinSelectionMessage(int skinIndex)
    {
        SkinIndex = HoolheyakSkinState.Normalize(skinIndex);
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteByte((byte)SkinIndex, 2);
    }

    public void Deserialize(PacketReader reader)
    {
        SkinIndex = HoolheyakSkinState.Normalize(reader.ReadByte(2));
    }

    public void HandleMessage(ulong senderId)
    {
        HoolheyakSkinState.SetSkin(senderId, SkinIndex);
        GD.Print($"[HoolheyakMod Skin] 收到玩家皮肤：NetId={senderId}, Skin={SkinIndex}");
    }
}
