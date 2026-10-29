using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using NuclearOption.ModdingApi;
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
    public override UniTask<ChatCommandResult> Execute(string[] args)
    {
        var players = UnitRegistry.playerLookupSteamId.Values;
        var playerNames = new string[players.Count];
        var i = 0;
        foreach (var player in players)
        {
            
            playerNames[i] = player.MentionArg;
            i++;
        }
        var response = $"[{players.Count - 1}] ";
        response += string.Join(", ", playerNames);
        return UniTask.FromResult(ChatCommandResult.Ok(response));
    }

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}
