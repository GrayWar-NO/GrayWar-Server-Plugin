using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Votekick command for players to vote for kicking someone.
/// </summary>
/// <param name="config"></param>
public class VoteKickCommand(ConfigFile config): PermissionConfigurableCommand(config)
{
    /// <inheritdoc />
    public override string Name { get; } = "votekick";

    /// <inheritdoc />
    public override string Description { get; } = "votes in favor of kicking a player from the  server.";

    /// <inheritdoc />
    public override string Usage { get; } = "votekick <player (by name or ID tag)>";

    /// <inheritdoc />
    public override bool Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public override bool Validate(string[] args)
    {
        return args.Length == 1 && PlayerUtils.TryFindPlayer(args[0], out _);
    }

    /// <inheritdoc />
    public override bool Execute(Player player, string[] args, out string? response)
    {
        PlayerUtils.TryFindPlayer(args[0], out var targetPlayer);
        if (targetPlayer! == player)
        {
            response = "You cannot vote for yourself!";
            return false;
        }
        GwServerPlugin.VoteKickService.AddVote(targetPlayer!.SteamID, player.SteamID);
        response = $"Successfully voted to kick {targetPlayer.PlayerName}.";
        return true;
    }

    /// <inheritdoc />
    public override bool Execute(string[] args, out string? response)
    {
        response = "Cannot use votekick from the console, use kick instead.";
        return false;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Everyone;
}