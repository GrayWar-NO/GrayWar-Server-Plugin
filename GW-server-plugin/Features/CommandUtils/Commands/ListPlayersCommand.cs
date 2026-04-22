using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using Mirage;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Lists the players on the server
/// </summary>
/// <param name="config"></param>
public class ListPlayersCommand(ConfigFile config) : PermissionConfigurableCommand(config), IConsoleCommand, IGameCommand
{
    /// <inheritdoc />
    public override string Name { get; } = "listplayers";

    /// <inheritdoc />
    public override string Description { get; } = "Returns the list of players currently on the server.";

    /// <inheritdoc />
    public override string Usage { get; } = "listplayers (takes no arguments)";

    /// <inheritdoc />
    public bool Validate(Player player, string[] args)
    {
        return Validate(args);
    }

    /// <inheritdoc />
    public bool Validate(string[] args)
    {
        return args.Length == 0;
    }

    /// <inheritdoc />
    public bool Execute(Player player, string[] args, out string? response) => Execute(args, out response);

    /// <inheritdoc />
    public bool Execute(string[] args, out string? response)
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
