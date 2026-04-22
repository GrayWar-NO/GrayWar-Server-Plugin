using System.Linq;
using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Features.IPC.Packets;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Sends a private message to a specific player.
/// </summary>
/// <param name="config"></param>
public class WhisperCommand(ConfigFile config) : PermissionConfigurableCommand(config)
{
    /// <inheritdoc />
    public override string Name { get; } = "whisper";

    /// <inheritdoc />
    public override string Description { get; } = "Send a private message to a specified user.";
    
    /// <inheritdoc />
    public override string Usage { get; } = "whisper <user / userID> <message>";

    /// <inheritdoc />
    public override bool Validate(Player player, string[] args)
    {
        return Validate(args);
    }

    /// <inheritdoc />
    public override bool Validate(string[] args)
    {
        return args.Length > 1;
    }

    /// <inheritdoc />
    public override bool Execute(Player player, string[] args, out string? response) =>
        Behaviour(player, args, out response);

    /// <inheritdoc />
    public override bool Execute(string[] args, out string? response) =>
        Behaviour(null, args, out response);
            

    private bool Behaviour(Player? sender, string[] args, out string response)
    {
        var found = PlayerUtils.TryFindPlayer(args[0], out var target);
        if (!found || !target)
        {
            response = $"Could not identify a player by \"{args[0]}\".";
            return false;
        }
        
        var message = string.Join(" ", args.Skip(1));
        ChatService.SendPrivateChatMessage(message, target!, sender?.PlayerName ?? PluginConfig.ServerBroadcastName!.Value);
        response = $"Message sent to {target!.PlayerName}:  {message}";
        
        var senderSteamId = sender?.SteamID ?? 0;
        
        var outPacket = new ChatLogPacket
        {
            SteamID = senderSteamId,
            ChatName = $"whisper({senderSteamId}:{target?.SteamID})",
            LogText = message
        };
        GwServerPlugin.LoggingOutBox.Add(outPacket);
        return true;
    }
    

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
    
    
    
}