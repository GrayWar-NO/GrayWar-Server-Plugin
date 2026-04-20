using BepInEx.Configuration;
using GW_server_plugin.Enums;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Tells something to everyone on the server
/// </summary>
/// <param name="config"></param>
public class TellCommand(ConfigFile config) : PermissionConfigurableCommand(config)
{
    /// <inheritdoc />
    public override string Name { get; } = "tell";

    /// <inheritdoc />
    public override string Description { get; } = "Broadcast a message to everyone on the server.";
    
    /// <inheritdoc />
    public override string Usage { get; } = "tell <message>";

    /// <inheritdoc />
    public override bool Validate(Player player, string[] args)
    {
        return Validate(args);
    }

    /// <inheritdoc />
    public override bool Validate(string[] args)
    {
        return args.Length > 0;
    }

    /// <inheritdoc />
    public override bool Execute(Player player, string[] args, out string? response)
    {
        var result = Execute(args, out var resp);
        response = resp;
        return result;
    }

    /// <inheritdoc />
    public override bool Execute(string[] args, out string? response)
    {
        var message = string.Join(" ", args);
        ChatService.SendChatMessageAsServer(message);
        response = null;
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
    
    
    
}