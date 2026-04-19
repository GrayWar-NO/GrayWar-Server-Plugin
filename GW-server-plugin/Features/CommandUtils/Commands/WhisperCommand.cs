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
    public override bool Execute(Player player, string[] args, out string? response)
    {
        var found = PlayerUtils.TryFindPlayer(args[0], out var target);
        if (!found || !target)
        {
            response = $"Could not identify a player by \"{args[0]}\".";
            return true;
        }
        
        var message = string.Join(" ", args.Skip(1));
        ChatService.SendPrivateChatMessage(message, target!, player.PlayerName);
        response = $"Message sent to {target}:  {message}";
        
        var outPacket = new ChatLogPacket
        {
            SteamID = player.SteamID,
            ChatName = $"whisper({player.SteamID}:{target?.SteamID})",
            LogText = message
        };
        GwServerPlugin.LoggingOutBox.Add(outPacket);
        return true;
    }

    /// <inheritdoc />
    public override bool Execute(string[] args, out string? response)
    {
        var found = PlayerUtils.TryFindPlayer(args[0], out var target);
        if (!found || !target)
        {
            response = $"Could not identify a player by \"{args[0]}\".";
            return true;
        }
        
        var message = string.Join(" ", args.Skip(1));
        ChatService.SendPrivateChatMessage(message, target!, "A moderator");
        response = $"Message sent to {target}:  {message}";
        
        var outPacket = new ChatLogPacket
        {
            SteamID = 0,
            ChatName = $"whisper({0}:{target?.SteamID})",
            LogText = message
        };
        GwServerPlugin.LoggingOutBox.Add(outPacket);
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
    
    
    
}