using Godot;
using HarmonyLib;
using HoolheyakMod.Code.Relics;
using HoolheyakMod.Scripts;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace HoolheyakMod.Code.Patches;

[HarmonyPatch(typeof(NClickableControl), nameof(NClickableControl._GuiInput))]
internal static class HistoryBookInspectPatch
{
    [HarmonyPrefix]
    private static bool Prefix(NClickableControl __instance, InputEvent __0)
    {
        if (__instance is not NRelicInventoryHolder holder ||
            __0 is not InputEventMouseButton { ButtonIndex: MouseButton.Right } mouse ||
            holder.Relic?.Model is not BeautifulHistoryBook { Owner: not null } book)
            return true;
        if (mouse.Pressed) NHistoryBookScreen.Show(book);
        __instance.AcceptEvent();
        return false;
    }
}
