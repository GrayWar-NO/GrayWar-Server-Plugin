using System;
using NuclearOption.Chat;
using NuclearOption.Networking;
using GW_server_plugin.Helpers;

namespace GW_server_plugin.Features;

/// <summary>
///     Manages server chat functionality.
/// </summary>
public static class ChatService
{
    private static bool CanSend(string message, bool ignoreEmpty = false, bool ignoreRateLimit = false)
    {
        if (string.IsNullOrWhiteSpace(message) && !ignoreEmpty)
        {
            GwServerPlugin.Logger.LogWarning("Cannot send empty chat message.");
            return false;
        }
        if (!Globals.ChatManagerInstance)
        {
            GwServerPlugin.Logger.LogWarning("Chat manager instance is null.");
            return false;
        }

        if (ignoreRateLimit)
            return true;
        
        try
        {
            return ChatManager.CanSend(message, true, true);
        }
        catch (ArgumentException e)
        {
            GwServerPlugin.Logger.LogError($"Cannot send chat message: {e.Message}");
            return false;
        }
    }

    /// <summary>
    ///     Sanitizes a chat message to prevent command injection.
    /// </summary>
    /// <param name="message"> The message to sanitize. </param>
    /// <returns> The sanitized message. </returns>
    private static string SanitizeMessage(this string message)
    {
        return message.TrimStart(PluginConfig.CommandPrefixChar);
    }

    /// <summary>
    ///     Pre-processes a chat message by replacing dynamic placeholders &amp; sanitizing it.
    /// </summary>
    /// <param name="message"> The message to pre-process. </param>
    /// <param name="player"> Player to use for chat message variables </param>
    /// <returns> The pre-processed message. </returns>
    private static string PreProcessMessage(this string message, Player? player = null)
    {
        return DynamicPlaceholderUtils.ReplaceDynamicPlaceholders(message, player).SanitizeMessage();
    }

    /// <summary>
    ///     Sends a chat message to all clients.
    /// </summary>
    /// <param name="message"> The message to send. </param>
    /// <param name="player"> Player to use for chat message variables </param>
    public static void SendChatMessage(string message, Player? player = null)
    {
        var actualMessage = "{server_broadcast_name} " + message;
        actualMessage = actualMessage.PreProcessMessage(player);
        
        if (!CanSend(actualMessage, ignoreRateLimit: true))
        {
            GwServerPlugin.Logger.LogWarning("Cannot send chat message.");
            return;
        }

        while (actualMessage.Length > 128)
        {
            Globals.ChatManagerInstance.RpcServerMessage(actualMessage.Substring(0, 128), false);
            actualMessage = actualMessage.Substring(128);
        }
        
        Globals.ChatManagerInstance.RpcServerMessage(actualMessage, false);
    }

    /// <summary>
    ///     Sends a private chat message to a player.
    /// </summary>
    /// <param name="message"> The message to send. </param>
    /// <param name="targetPlayer"> The player to send the message to. </param>
    public static void SendPrivateChatMessage(string message, Player targetPlayer)
    {
        var actualMessage = message.PreProcessMessage(targetPlayer);
        
        if (!CanSend(actualMessage, ignoreRateLimit: true))
        {
            GwServerPlugin.Logger.LogWarning("Cannot send private chat message.");
            return;
        }
        while (actualMessage.Length > 128)
        {
            Globals.ChatManagerInstance.TargetReceiveMessage(targetPlayer.Owner, actualMessage, targetPlayer, true);
            actualMessage = actualMessage.Substring(128);
        }

        Globals.ChatManagerInstance.TargetReceiveMessage(targetPlayer.Owner, actualMessage, targetPlayer, true);
        GwServerPlugin.Logger.LogInfo($"Sent private message to {targetPlayer.PlayerName}: {actualMessage}");
    }
}