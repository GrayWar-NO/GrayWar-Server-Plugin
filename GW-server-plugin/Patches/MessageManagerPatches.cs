using GW_server_plugin.Events;
using HarmonyLib;
using NuclearOption.Networking;

namespace GW_server_plugin.Patches;

[HarmonyPatch(typeof(MessageManager))]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
internal static class MessageManagerPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MessageManager.JoinMessage))]
    private static void JoinMessagePostfix(Player joinedPlayer)
    {
        PlayerEvents.OnPlayerJoined(joinedPlayer);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MessageManager.DisconnectedMessage))]
    private static void DisconnectedMessagePostfix(Player player)
    {
        PlayerEvents.OnPlayerLeft(player);
    }
}