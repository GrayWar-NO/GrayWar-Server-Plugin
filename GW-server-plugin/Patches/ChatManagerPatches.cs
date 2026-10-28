using System;
using Com.Graywar.NoServerManager.Proto;
using Google.Protobuf.WellKnownTypes;
using HarmonyLib;
using Mirage;
using NuclearOption.Chat;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Patches;

[HarmonyPatch(typeof(ChatManager))]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
internal static class ChatManagerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch("UserCode_CmdSendChatMessage_\u002D456754112")]
    private static void UserCode_CmdSendChatMessagePrefix(string message, bool allChat, INetworkPlayer sender)
    {
        if (!sender.TryGetPlayer(out var player) || player == null)
        {
            GwServerPlugin.Logger.LogWarning("Player component is null");
            return;
        }

        var factionName = player.HQ?.faction?.factionName ?? "unknown";
        GwServerPlugin.Logger.LogInfo(allChat
            ? $"{player.GetDisplayName()} sent message: {message}"
            : $"{player.GetDisplayName()} sent message in {factionName} chat: {message}");
        
        var log = new ChatLog
        {
            MessageChannel = allChat ? "all" : factionName,
            MessageSendTime = DateTime.UtcNow.ToTimestamp(),
            Message = message,
            SenderSteamID = player.SteamID
        };
        GwServerPlugin.GrpcMgr.ChatLogStream?.WriteAsync(log);
    }
}
