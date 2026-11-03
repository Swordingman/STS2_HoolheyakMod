using BaseLib.Abstracts;
using Godot;
using HoolheyakMod.Scripts.Utils;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace HoolheyakMod.Code.Networking;

public sealed class SkinSelectionMessage : ICustomMessage
{
    public int SkinIndex { get; private set; }
    public bool RequestReply { get; private set; }

    public bool ShouldBroadcast => true;
    public bool ShouldBuffer => false;

    public SkinSelectionMessage() { }

    public SkinSelectionMessage(int skinIndex, bool requestReply = false)
    {
        SkinIndex = HoolheyakSkinState.Normalize(skinIndex);
        RequestReply = requestReply;
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteByte((byte)SkinIndex, 2);
        writer.WriteBool(RequestReply);
    }

    public void Deserialize(PacketReader reader)
    {
        SkinIndex = HoolheyakSkinState.Normalize(reader.ReadByte(2));
        RequestReply = reader.ReadBool();
    }

    public void HandleMessage(ulong senderId)
    {
        HoolheyakSkinNetwork.Receive(senderId, SkinIndex, RequestReply);
    }
}
