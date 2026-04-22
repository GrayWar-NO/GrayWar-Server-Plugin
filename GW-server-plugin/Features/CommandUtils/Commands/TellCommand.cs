using BepInEx.Configuration;
using GW_server_plugin.Enums;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Tells something to everyone on the server
/// </summary>
/// <param name="config"></param>
public class TellCommand(ConfigFile config) : PermissionConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string Name { get; } = "tell";

    /// <inheritdoc />
    public override string Description { get; } = "Broadcast a message to everyone on the server.";
    
    /// <inheritdoc />
    public override string Usage { get; } = "tell <message>";

    /// <inheritdoc />
    public bool Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public bool Validate(string[] args)
    {
        return args.Length > 0;
    }

    /// <inheritdoc />
    public bool Execute(Player player, string[] args, out string? response)
    {
        var result = Execute(args, out var resp);
        response = resp;
        return result;
    }

    /// <inheritdoc />
    public bool Execute(string[] args, out string? response)
    {
        var message = string.Join(" ", args);
        ChatService.SendChatMessageAsServer(message);
        response = null;
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
    
    
    
}