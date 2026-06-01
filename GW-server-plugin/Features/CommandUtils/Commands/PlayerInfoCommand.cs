using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
///     Command to get player name and ID from their steamID.
/// </summary>
/// <param name="config"></param>
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
    public bool Validate(string[] args)
    {
        return args.Length == 1 && ulong.TryParse(args[0], out _);
    }

    /// <inheritdoc />
    public bool Execute(string[] args, out string? response)
    {
        ulong.TryParse(args[0], out var steamID);
        if (!PlayerUtils.TryFindPlayerBySteamId(steamID, out var player))
        {
            response = $"Player for steamID {steamID} not found";
            return false;
        }
        response = player!.PlayerName;
        return true;
    }
}
