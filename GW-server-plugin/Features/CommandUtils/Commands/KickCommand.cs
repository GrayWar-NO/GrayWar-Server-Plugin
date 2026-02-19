using System.Linq;
using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to kick a player from the server
/// </summary>
/// <param name="config"></param>
public class KickCommand(ConfigFile config): PermissionConfigurableCommand(config)
{
    /// <inheritdoc />
    public override string Name { get; } = "kick";

    /// <inheritdoc />
    public override string Description { get; } = "Kicks a player from the  server.";

    /// <inheritdoc />
    public override string Usage { get; } = "kick <player (by name, steamID or ID tag)> <reason>";

    /// <inheritdoc />
    public override bool Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public override bool Validate(string[] args)
    {
        return args.Length >= 2 && (PlayerUtils.TryFindPlayer(args[0], out _) || ulong.TryParse(args[0], out _));
    }

    /// <inheritdoc />
    public override bool Execute(Player player, string[] args, out string? response)
    {
        var target = args[0];
        PlayerUtils.TryFindPlayer(target, out var targetPlayer);
        if (targetPlayer != player) return Execute(args, out response);
        response = "You cannot kick yourself!";
        return false;
    }

    /// <inheritdoc />
    public override bool Execute(string[] args, out string? response)
    {
        var target = args[0];
        if (PlayerUtils.TryFindPlayer(target, out var targetPlayer))
        {
            PlayerUtils.KickPlayer(targetPlayer!, string.Join(" ", args.Skip(1).ToArray()));
            response = $"{targetPlayer!.PlayerName} has been kicked!";
            return true;
        }
        response = $"{target} is not a valid player!";
        return false;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
    
}