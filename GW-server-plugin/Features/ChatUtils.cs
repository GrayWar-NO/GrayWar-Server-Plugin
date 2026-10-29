using System.Text.RegularExpressions;
using NuclearOption.Chat;
using NuclearOption.Networking;
using GW_server_plugin.Helpers;
using NuclearOption.ModdingApi;

namespace GW_server_plugin.Features;

/// <summary>
///     Manages server chat functionality.
/// </summary>
public static class ChatUtils
{
    /// <summary>
    ///     Sends a chat message to all clients.
    /// </summary>
    /// <param name="message"> The message to send. </param>
    public static void SendChatMessageAsServer(string message)
    {
        ServerChat.SendToAll($"{PluginConfig.ServerBroadcastName!.Value}: {message}", true);
    }

    /// <summary>
    ///     Sends a private chat message to a player (sender visible).
    /// </summary>
    /// <param name="message"> The message to send. </param>
    /// <param name="targetPlayer"> The player to send the message to. </param>
    /// <param name="sender"> The player that sends the message. </param>
    public static void SendPrivateChatMessage(string message, Player targetPlayer, Player? sender = null)
    {
        var actualMessage = sender == null ? $"{PluginConfig.ServerBroadcastName!.Value}: {message}" : $"{sender.MentionArg}: {message}";

        ServerChat.SendToPlayer(new PlayerHandle(targetPlayer), actualMessage, true);
        
        GwServerPlugin.Logger.LogInfo($"Sent private message to {targetPlayer.GetLogName()}: {actualMessage}");
    }
    
    /// <summary>
    ///     Replaces all mentions in <paramref name="message"/> with the name of the player associated with them.
    /// </summary>
    /// <param name="message">message to change</param>
    /// <returns>number of mentions replaced.</returns>
    public static string ReplaceMentionsWithNames(string message)
    {
        if (string.IsNullOrWhiteSpace(message) || !message.Contains("[@"))
            return message;
        
        return Regex.Replace(message, @"\[@\d+\]", match =>
        {
            var mention = match.Value;
            if (!ChatHelper.TryReadMention(mention, 0, out var steamId, out _) ||
                !PlayerUtils.TryFindPlayerBySteamId(steamId.m_SteamID, out var player))
                return match.Value;
            return player!.GetDisplayName();
        });
    }
    
}
