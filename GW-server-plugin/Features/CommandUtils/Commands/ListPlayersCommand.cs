using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using Mirage;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

public class ListPlayersCommand(ConfigFile config) : PermissionConfigurableCommand(config)
{
    /// <inheritdoc />
    public override string Name { get; } = "listplayers";

    /// <inheritdoc />
    public override string Description { get; } = "Returns the list of players currently on the server.";

    /// <inheritdoc />
    public override string Usage { get; } = "listplayers (takes no arguments)";

    /// <inheritdoc />
    public override bool Validate(Player player, string[] args)
    {
        return Validate(args);
    }

    /// <inheritdoc />
    public override bool Validate(string[] args)
    {
        return args.Length == 0;
    }

    /// <inheritdoc />
    public override bool Execute(Player player, string[] args, out string? response)
    {
        return Execute(args, out response);
    }

    /// <inheritdoc />
    public override bool Execute(string[] args, out string? response)
    {
        var players = Globals.AuthenticatedPlayers;
        var playerNames = "";
        foreach (INetworkPlayer player in players)
        {
            Player? p;
            player.TryGetPlayer(out p);
            if (p != null)
            {
                playerNames += $"{p.PlayerName}, ";
            }
        }
        response = $"[{players.Count - 1}] ";
        response += playerNames;
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
}
