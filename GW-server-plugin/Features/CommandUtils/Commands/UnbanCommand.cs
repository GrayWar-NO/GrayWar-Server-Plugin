using System;
using System.Security;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;
using Steamworks;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to ban a player.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class UnbanCommand(ConfigFile config) : PermissionConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string Name { get; } = "unban";

    /// <inheritdoc />
    public override string Description { get; } = "Unbans a player from the server.";

    /// <inheritdoc />
    public override string Usage { get; } = "unban <Player (by name, steamID or playerID)>";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(args.Length == 1 &&
                                  (PlayerUtils.TryFindPlayer(args[0], out _) || ulong.TryParse(args[0], out _)));
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Execute(args);

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        string? response;
        var target = args[0];
        ulong banSteamID;
        if (ulong.TryParse(target, out var targetID) &&
            targetID > (ulong)Globals.DedicatedServerManagerInstance.Config.MaxPlayers)
        {
            banSteamID = targetID;
            response = $"Unbanned player with steamID {banSteamID}";
        }
        else
        {
            var rs = PlayerUtils.TryFindPlayer(target, out var player);
            if (!rs)
                throw new VerificationException(
                    $"Could not find player {target}: validation was not called properly.");
            banSteamID = player!.SteamID;
            response = $"Unbanned player {player.PlayerName}";
        }

        AllowBanListUtils.UnbanAndRemoveId(
            Globals.NetworkManagerNuclearOptionInstance.Authenticator.BanList,
            Globals.DedicatedServerManagerInstance.Config.BanListPaths[0],
            new CSteamID(banSteamID));
        var log = new BanRequest
        {
            SteamID = banSteamID,
            BanEnd = DateTime.UtcNow.ToTimestamp(),
            ShouldBeBanned = false
        };
        GwServerPlugin.GrpcMgr.TrySendBan(log);
        return UniTask.FromResult<(bool, string?)>((true, response));
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
}