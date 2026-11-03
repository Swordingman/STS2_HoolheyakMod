using System;
using System.Collections.Generic;
using BaseLib.Abstracts;
using Godot;
using HoolheyakMod.Scripts.Utils;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
#if STS2_BETA
using LobbyPlayerCompat = MegaCrit.Sts2.Core.Entities.Multiplayer.StartRunLobbyPlayer;
#else
using LobbyPlayerCompat = MegaCrit.Sts2.Core.Entities.Multiplayer.LobbyPlayer;
#endif

namespace HoolheyakMod.Code.Networking;

internal static class HoolheyakSkinNetwork
{
    private static StartRunLobby? _activeLobby;
    private static LoadRunLobby? _activeLoadLobby;
    private static INetGameService? ActiveService => _activeLoadLobby?.NetService ?? _activeLobby?.NetService;
    private static readonly Dictionary<StartRunLobby, Action<LobbyPlayerCompat>> ConnectedHandlers = new();
    private static readonly HashSet<INetGameService> Registrations = new();

    private static void Register(INetGameService service)
    {
        if (Registrations.Add(service))
            service.RegisterMessageHandler<CustomMessageWrapper>(HandleCustomMessage);
    }

    public static void Attach(StartRunLobby lobby)
    {
        if (ConnectedHandlers.ContainsKey(lobby)) return;
        HoolheyakSkinState.Clear();
        _activeLobby = lobby;
        Register(lobby.NetService);
        Action<LobbyPlayerCompat> handler = _ => PublishLocalSkin(lobby);
        ConnectedHandlers[lobby] = handler;
        lobby.PlayerConnected += handler;
        GD.Print("[HoolheyakMod Skin] 新建大厅皮肤同步已连接。");
    }

    public static void Attach(LoadRunLobby lobby)
    {
        _activeLoadLobby = lobby;
        Register(lobby.NetService);
        HoolheyakSkinState.Clear();
        foreach (var player in lobby.Run.Players) HoolheyakSkinState.TryRestore(player);
        GD.Print("[HoolheyakMod Skin] 继续存档大厅皮肤已恢复，等待玩家确认。");
    }

    private static void Unregister(INetGameService service)
    {
        if (Registrations.Remove(service))
            service.UnregisterMessageHandler<CustomMessageWrapper>(HandleCustomMessage);
    }

    public static void Detach(StartRunLobby lobby)
    {
        if (ConnectedHandlers.Remove(lobby, out var handler)) lobby.PlayerConnected -= handler;
        Unregister(lobby.NetService);
        if (ReferenceEquals(_activeLobby, lobby)) _activeLobby = null;
    }

    public static void Detach(LoadRunLobby lobby)
    {
        Unregister(lobby.NetService);
        if (ReferenceEquals(_activeLoadLobby, lobby)) _activeLoadLobby = null;
    }

    internal static int GetLocalSelection()
    {
        var service = ActiveService;
        return service != null && HoolheyakSkinState.TryGetSkin(service.NetId, out int skin)
            ? skin : HoolheyakSkinState.Normalize(HoolheyakConfig.CurrentSkinIndex);
    }

    public static void SelectLocalSkin(int skin)
    {
        HoolheyakConfig.CurrentSkinIndex = HoolheyakSkinState.Normalize(skin);
        if (ActiveService is not { } service) return;
        HoolheyakSkinState.SetSkin(service.NetId, skin);
        Publish(service, true);
    }

    public static void PublishLocalSkin(StartRunLobby lobby) => Publish(lobby.NetService, true);
    public static void PublishLocalSkin(LoadRunLobby lobby) => Publish(lobby.NetService, true);
    public static void PublishLocalSkin()
    {
        if (ActiveService is { } service) Publish(service, true);
    }

    private static void Publish(INetGameService service, bool requestReply)
    {
        int skin = GetLocalSelection();
        HoolheyakSkinState.SetSkin(service.NetId, skin);
        StoreLoadedSelection(service.NetId, skin);
        CustomMessageWrapper.Send(new SkinSelectionMessage(skin, requestReply), service);
        GD.Print($"[HoolheyakMod Skin] 发送玩家皮肤：NetId={service.NetId}, Skin={skin}, RequestReply={requestReply}");
    }

    private static void StoreLoadedSelection(ulong playerId, int skin)
    {
        if (_activeLoadLobby == null) return;
        foreach (var player in _activeLoadLobby.Run.Players)
            if (player.NetId == playerId) HoolheyakSkinState.Store(player, skin);
    }

    internal static void Receive(ulong senderId, int skin, bool requestReply)
    {
        HoolheyakSkinState.SetSkin(senderId, skin);
        StoreLoadedSelection(senderId, skin);
        GD.Print($"[HoolheyakMod Skin] 收到玩家皮肤：NetId={senderId}, Skin={skin}");
        // A ready client asks once; the reply never asks again, avoiding echo loops.
        if (requestReply && ActiveService is { } service && senderId != service.NetId)
            Publish(service, false);
    }

    private static void HandleCustomMessage(CustomMessageWrapper wrapper, ulong senderId)
    {
        if (wrapper.Message is SkinSelectionMessage message) message.HandleMessage(senderId);
    }
}
