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

[HarmonyPatch]
internal static class LoadRunLobbyConstructorSkinPatch
{
    private static IEnumerable<MethodBase> TargetMethods() => typeof(LoadRunLobby).GetConstructors();
    [HarmonyPostfix]
    private static void Postfix(LoadRunLobby __instance) => HoolheyakSkinNetwork.Attach(__instance);
}

[HarmonyPatch(typeof(LoadRunLobby), nameof(LoadRunLobby.CleanUp))]
internal static class LoadRunLobbyCleanUpSkinPatch
{
    [HarmonyPrefix]
    private static void Prefix(LoadRunLobby __instance) => HoolheyakSkinNetwork.Detach(__instance);
}

[HarmonyPatch(typeof(StartRunLobby), nameof(StartRunLobby.SetReady))]
internal static class StartRunLobbyReadySkinPatch
{
    [HarmonyPrefix]
    private static void Prefix(StartRunLobby __instance, bool __0)
    { if (__0) HoolheyakSkinNetwork.PublishLocalSkin(__instance); }
}

[HarmonyPatch(typeof(LoadRunLobby), nameof(LoadRunLobby.SetReady))]
internal static class LoadRunLobbyReadySkinPatch
{
    [HarmonyPrefix]
    private static void Prefix(LoadRunLobby __instance, bool __0)
    { if (__0) HoolheyakSkinNetwork.PublishLocalSkin(__instance); }
}
