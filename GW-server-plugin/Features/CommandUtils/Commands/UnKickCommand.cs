using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;
using Steamworks;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to kick a player from the server
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class UnKickCommand(ConfigFile config) : PermissionConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string Name => "unkick";

    /// <inheritdoc />
    public override string Description => "Unkicks a player from the  server.";

    /// <inheritdoc />
    public override string Usage => "unkick <player (by steamID only)>";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(args.Length == 1 && ulong.TryParse(args[0], out _));
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Execute(args);

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        UnKickPlayer(ulong.Parse(args[0]));
        return UniTask.FromResult<(bool, string?)>((true, $"Unkicked player with steamID {args[0]}"));
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;

    private static void UnKickPlayer(ulong steamID)
    {
        Globals.NetworkManagerNuclearOptionInstance.Authenticator.KickList.Remove(new CSteamID(steamID));
        Globals.NetworkManagerNuclearOptionInstance.Authenticator.MissionKickList.Remove(new CSteamID(steamID));
    }
}