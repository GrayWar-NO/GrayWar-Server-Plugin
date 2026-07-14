using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
///     Command to get player name and ID from their steamID.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class PlayerInfoCommand(ConfigFile config): PermissionConfigurableCommand(config), IConsoleCommand
{
    /// <inheritdoc />
    public override string Name => "playerinfo";

    /// <inheritdoc />
    public override string Description => "Gets a player's name from their steamID";

    /// <inheritdoc />
    public override string Usage => "playerinfo <SteamID>";

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(args.Length == 1 && ulong.TryParse(args[0], out _));
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        string? response;
        ulong.TryParse(args[0], out var steamID);
        if (!PlayerUtils.TryFindPlayerBySteamId(steamID, out var player))
        {
            response = $"Player for steamID {steamID} not found";
            return UniTask.FromResult<(bool, string?)>((false, response));
        }
        response = player!.PlayerName;
        return UniTask.FromResult<(bool, string?)>((true, response));
    }
}
