using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Moderation;

/// <summary>
///     Command to get player name and ID from their steamID.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class PlayerInfoCommand(ConfigFile config): ConfigurableCommand(config), IConsoleCommand
{
    /// <inheritdoc />
    public override string Name => "playerinfo";

    /// <inheritdoc />
    public override string Description => "Gets a player's name from their steamID";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("player", ChatArgumentType.Player)
    ];

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;


    /// <inheritdoc />
    public UniTask<ChatCommandResult> Execute(string[] args)
    {
        string? response;
        ulong.TryParse(args[0], out var steamID);
        if (!PlayerUtils.TryFindPlayerBySteamId(steamID, out var player))
        {
            response = $"Player for steamID {steamID} not found";
            return UniTask.FromResult(ChatCommandResult.ClientError(response));
        }
        
        
        response = player!.MentionArg;
        return UniTask.FromResult(ChatCommandResult.Ok(response));
    }
}
