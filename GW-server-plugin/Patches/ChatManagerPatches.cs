using System;
using System.Linq;
using System.Text.RegularExpressions;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using HarmonyLib;
using Mirage;
using NuclearOption.Chat;
using GW_server_plugin.Features;
using GW_server_plugin.Features.CommandUtils;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Patches;

[HarmonyPatch(typeof(ChatManager))]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
internal static class ChatManagerPatches
{
    private static async UniTaskVoid ExecuteCommandAndRespondAsync(string commandName, string[] arguments, Player player)
    {
        try
        {
            var executionResult = await CommandService.TryExecuteCommand(commandName, arguments, player);
            var response = executionResult.response;
            if (!string.IsNullOrEmpty(response))
            {
                foreach (var s in response!.Split('\n'))
                {
                    ChatService.SendPrivateChatMessage(s, player);
                }
            }
        }
        catch (Exception ex)
        {
            GwServerPlugin.Logger.LogError($"Error executing async command '{commandName}': {ex.Message}");
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch("UserCode_CmdSendChatMessage_\u002D456754112")]
    private static bool UserCode_CmdSendChatMessagePrefix(string message, bool allChat, INetworkPlayer sender)
    {
        if (!sender.TryGetPlayer(out var player) || player == null)
        {
            GwServerPlugin.Logger.LogWarning("Player component is null");
            return false;
        }
        if (message.StartsWith(PluginConfig.CommandPrefix!.Value) && message.Length > 1)
        {
            var input = message.Remove(0,1);
            var matches = Regex.Matches(input, """[\"].+?[\"]|'[^']+'|\S+""");

            var arguments = (from Match m in matches select m.Value.Trim('"', '\'')).ToArray();

            var commandName = arguments[0];
            arguments = arguments.Skip(1).ToArray();
            
            ExecuteCommandAndRespondAsync(commandName, arguments, player).Forget();
            return false;
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
        
        // --- SERVER SIDE MESSAGE VALIDATION ---
        // This is taken from the original function that we skip.
        if (!Globals.ChatManagerInstance.ValidateChatMessageSize(message))
        {
            sender.SetError(1, NuclearOptionPlayerErrorFlags.MessagingSpam);
        }
        else
        {
            Player player1;
            if (!sender.TryGetPlayer<Player>(out player1))
            {
                sender.SetError(100, PlayerErrorFlags.Critical);
                sender.Disconnect();
            }
            else if (!Globals.ChatManagerInstance.CheckRateLimit(player1, true, true))
            {
                sender.SetError(1, NuclearOptionPlayerErrorFlags.MessagingSpam);
            }
        }
        // ---
        
        var displayName = player.GetColoredDisplayName();
        if (allChat)
        {
            Globals.ChatManagerInstance.RpcServerMessage($"{displayName}: {message}", true);
        }
        else
        {
            var senderHq = player.HQ;
            if (senderHq == null)
            {
                GwServerPlugin.Logger.LogWarning($"Could not send team chat for {player.SteamID}: player has no faction HQ.");
                return false;
            }

            foreach (var authenticatedPlayer in Globals.AuthenticatedPlayers)
            {
                if (!authenticatedPlayer.TryGetPlayer(out var recipient) || recipient?.HQ != senderHq)
                    continue;

                Globals.ChatManagerInstance.RpcTargetServerMessage(authenticatedPlayer, $"<color=#57ffff>(ally)</color> {displayName}: {message}", true);
            }
        }
        return false;
    }
}
