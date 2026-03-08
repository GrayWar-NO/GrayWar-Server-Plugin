using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using Mirage;
using NuclearOption.Chat;
using GW_server_plugin.Features;
using GW_server_plugin.Features.CommandUtils;
using GW_server_plugin.Features.IPC.Packets;
using GW_server_plugin.Helpers;
using Newtonsoft.Json;

namespace GW_server_plugin.Patches;

[HarmonyPatch(typeof(ChatManager))]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
internal static class ChatManagerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch("UserCode_CmdSendChatMessage_\u002D456754112")]
    private static bool UserCode_CmdSendChatMessagePrefix(string message, bool allChat, INetworkPlayer sender)
    {
        if (!sender.TryGetPlayer(out var player))
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

            var commandResult = CommandService.TryExecuteCommand(commandName, arguments, player!, out var response);
            if (response is not null)
            {
                foreach (var s in response.Split('\n'))
                {
                    ChatService.SendPrivateChatMessage(s, player!);
                }
            }
            if (commandResult) return false;
        }
        GwServerPlugin.Logger.LogInfo(allChat
            ? $"{player!.PlayerName} sent message: {message}"
            : $"{player!.PlayerName} sent message in {player.HQ.faction.factionName} chat: {message}");
        
        var outPacket = new ChatLogPacket
        {
            ChatName = allChat ? "all" : player.HQ.faction.factionName.ToLower(),
            LogText = message
        };
        GwServerPlugin.SocketOutBox.Add(JsonConvert.SerializeObject(outPacket));
        return true;
    }
}