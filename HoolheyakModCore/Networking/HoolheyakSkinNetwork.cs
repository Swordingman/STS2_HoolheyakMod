using System;
using System.Collections.Generic;
using BaseLib.Abstracts;
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

    private sealed class Registration
    {
        public StartRunLobby Lobby { get; }
        public Action<LobbyPlayerCompat> PlayerConnectedHandler { get; }

        public Registration(StartRunLobby lobby, Action<LobbyPlayerCompat> playerConnectedHandler)
        {
            Lobby = lobby;
            PlayerConnectedHandler = playerConnectedHandler;
        }
    }

    private static readonly Dictionary<INetGameService, Registration> Registrations = new();

    public static void Attach(StartRunLobby lobby)
    {
        var netService = lobby.NetService;
        if (Registrations.ContainsKey(netService)) return;

        _activeLobby = lobby;

        netService.RegisterMessageHandler<CustomMessageWrapper>(HandleCustomMessage);

        Action<LobbyPlayerCompat> playerConnectedHandler = _ => PublishLocalSkin(lobby);

        lobby.PlayerConnected += playerConnectedHandler;

        Registrations[netService] = new Registration(lobby, playerConnectedHandler);

        Console.WriteLine("[HoolheyakMod Skin] 已连接角色大厅皮肤同步。");
    }

    public static void Detach(StartRunLobby lobby)
    {
        var netService = lobby.NetService;

        if (!Registrations.Remove(netService, out var registration))
            return;

        registration.Lobby.PlayerConnected -= registration.PlayerConnectedHandler;
        netService.UnregisterMessageHandler<CustomMessageWrapper>(HandleCustomMessage);

        if (ReferenceEquals(_activeLobby, lobby))
            _activeLobby = null;

        Console.WriteLine("[HoolheyakMod Skin] 已断开角色大厅皮肤同步。");
    }

    public static void PublishLocalSkin(StartRunLobby lobby)
    {
        ulong localNetId = lobby.NetService.NetId;
        int skinIndex = HoolheyakSkinState.Normalize(HoolheyakConfig.CurrentSkinIndex);

        HoolheyakSkinState.SetSkin(localNetId, skinIndex);
        CustomMessageWrapper.Send(new SkinSelectionMessage(skinIndex), lobby.NetService);

        Console.WriteLine($"[HoolheyakMod Skin] 发送本地皮肤：NetId={localNetId}, Skin={skinIndex}");
    }

    public static void PublishLocalSkin()
    {
        if (_activeLobby == null)
        {
            Console.WriteLine("[HoolheyakMod Skin] 当前没有可用的角色大厅，未发送皮肤。");
            return;
        }

        PublishLocalSkin(_activeLobby);
    }

    private static void HandleCustomMessage(CustomMessageWrapper wrapper, ulong senderId)
    {
        if (wrapper.Message is SkinSelectionMessage message)
            message.HandleMessage(senderId);
    }
}
