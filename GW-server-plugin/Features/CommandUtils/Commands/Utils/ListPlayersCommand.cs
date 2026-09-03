using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using Mirage;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands.Utils;

/// <summary>
/// Lists the players on the server
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class ListPlayersCommand(ConfigFile config) : ConfigurableCommand(config), IConsoleCommand, IGameCommand
{
    /// <inheritdoc />
    public override string OutputName => "listplayers";

    /// <inheritdoc />
    public override string Description => "Returns the list of players currently on the server.";

    /// <inheritdoc />
    public override string Usage => "listplayers (takes no arguments)";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args) => UniTask.FromResult(args.Length == 0);
    

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Execute(args);

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var players = Globals.AuthenticatedPlayers;
        var playerNames = "";
        foreach (INetworkPlayer player in players)
        {
            Player? p;
            player.TryGetPlayer(out p);
            if (p != null)
            {
                playerNames += $"{p.GetDisplayName()}, ";
            }
        }
        var response = $"[{players.Count - 1}] ";
        response += playerNames;
        return UniTask.FromResult<(bool, string?)>((true, response));
    }

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}
