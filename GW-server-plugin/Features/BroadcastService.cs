using System;

namespace GW_server_plugin.Features;

/// <summary>
/// Class that declares functions related to the server messages broadcast.
/// </summary>
public static class BroadcastService
{
    /// <summary>
    /// Sends a message from the list of broadcasts to the server.
    /// </summary>
    public static void SendBroadcast()
    {
        if (PluginConfig.NBroadcastMessages!.Value == 0) return;
        var random = new Random();
        var index = random.Next(PluginConfig.BroadcastMessages.Count);
        ChatService.SendChatMessageAsServer(PluginConfig.BroadcastMessages[index]!.Value);
    }
}