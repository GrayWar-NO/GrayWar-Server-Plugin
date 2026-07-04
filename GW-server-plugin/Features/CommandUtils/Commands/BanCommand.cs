using System.Linq;
using System.Security;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to ban a player.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class BanCommand(ConfigFile config) : PermissionConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string Name { get; } = "ban";

    /// <inheritdoc />
    public override string Description { get; } = "Bans a player from the server.";

    /// <inheritdoc />
    public override string Usage { get; } =
        "ban <Player (by name, steamID or playerID)> <Optional string Reason> <Optional duration (Xh or Xd)>";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(
            args.Length >= 1 && 
            (PlayerUtils.TryFindPlayer(args[0], out _) || ulong.TryParse(args[0], out _)));
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        var target = args[0];
        PlayerUtils.TryFindPlayer(target, out var targetPlayer);
        if (targetPlayer != player) return Execute(args);
        return UniTask.FromResult<(bool, string?)>((false, "You cannot ban yourself!"));
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var target = args[0];
        string? duration = null;
        string reason;
        if (args.Length > 1)
        {
            var tmp = args[args.Length - 1].Last();
            duration = tmp is 'd' or 'h' ? args[args.Length - 1] : null;
            var reasonEnum = args.Skip(1).ToList();
            if (duration is not null)
            {
                reasonEnum.RemoveAt(reasonEnum.Count - 1);
            }

            reason = string.Join(" ", reasonEnum);
        }
        else
        {
            reason = "Unknown reason";
        }

        string? response;

        ulong banSteamID;
        if (ulong.TryParse(target, out var targetID) &&
            targetID > (ulong)Globals.DedicatedServerManagerInstance.Config.MaxPlayers)
        {
            banSteamID = targetID;
            response = $"Banned player with steamID {banSteamID} for reason {reason}";
            if (PlayerUtils.TryFindPlayerBySteamId(banSteamID, out var targetPlayer))
            {
                Globals.NetworkManagerNuclearOptionInstance
                    .KickPlayerAsync(targetPlayer!, $"Banned for reason: {reason}").Forget();
            }
        }
        else
        {
            var rs = PlayerUtils.TryFindPlayer(target, out var player);
            if (!rs)
                throw new VerificationException(
                    $"Could not find player {target}: validation was not called properly.");
            banSteamID = player!.SteamID;
            Globals.NetworkManagerNuclearOptionInstance
                .KickPlayerAsync(player, $"Banned for reason: {reason}").Forget();
            response = $"Banned player {player.PlayerName} for reason {reason}";
        }

        if (duration is not null)
        {
            response += $" for {duration}.";
        }

        PlayerUtils.BanPlayer(banSteamID, reason, duration);
        if (!GwServerPlugin.FamilySharingBorrowers.TryGetValue(banSteamID, out var ownerSteamID)) UniTask.FromResult<(bool, string?)>((true, response));
        if (ownerSteamID == banSteamID) return UniTask.FromResult<(bool, string?)>((true, response));
        PlayerUtils.BanPlayer(ownerSteamID, $"Owner of family shared banned account. Child banned for {reason}",
            duration);
        response += $"\tBanned Owner with steamID {ownerSteamID} as well";
        return UniTask.FromResult<(bool, string?)>((true, response));
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
}