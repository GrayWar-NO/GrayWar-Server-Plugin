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
    [HarmonyPatch(typeof(MessageManager), nameof(MessageManager.JoinMessage))]
    private static void JoinMessagePostfix(Player joinedPlayer)
    {
        PlayerEvents.OnPlayerJoined(joinedPlayer);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MessageManager), nameof(MessageManager.DisconnectedMessage))]
    private static void DisconnectedMessagePostfix(Player player)
    {
        PlayerEvents.OnPlayerLeft(player);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MessageManager), nameof(MessageManager.RpcPlayerJoinFactionMessage))]
    private static void JoinFactionPostfix(Player player, FactionHQ hq)
    {
        PlayerEvents.OnPlayerJoinFaction(player, hq);
    }
}