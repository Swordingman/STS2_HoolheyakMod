using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

namespace HoolheyakMod.Code.Networking;

[HarmonyPatch]
internal static class StartRunLobbyConstructorPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        return typeof(StartRunLobby).GetConstructors(BindingFlags.Public | BindingFlags.Instance);
    }

    [HarmonyPostfix]
    private static void Postfix(StartRunLobby __instance)
    {
        HoolheyakSkinNetwork.Attach(__instance);
    }
}

[HarmonyPatch(typeof(StartRunLobby), nameof(StartRunLobby.CleanUp))]
internal static class StartRunLobbyCleanUpPatch
{
    [HarmonyPrefix]
    private static void Prefix(StartRunLobby __instance)
    {
        HoolheyakSkinNetwork.Detach(__instance);
    }
}
