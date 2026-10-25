using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using Mirage;
using NuclearOption.ModdingApi;
using NuclearOption.Networking;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Utils;

/// <summary>
/// Lists the players on the server
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class ListPlayersCommand(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string Name => "listplayers";

    /// <inheritdoc />
    public override string Description => "Returns the list of players currently on the server.";
    
    /// <inheritdoc />
    public override ChatCommandResult Execute(string[] args)
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
        return ChatCommandResult.Ok(response);
    }

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}
