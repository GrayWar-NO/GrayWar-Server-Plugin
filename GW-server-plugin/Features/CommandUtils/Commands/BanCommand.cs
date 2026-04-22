using System.Security;
using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to ban a player.
/// </summary>
/// <param name="config"></param>
public class BanCommand(ConfigFile config): PermissionConfigurableCommand(config), IConsoleCommand, IGameCommand
{
    /// <inheritdoc />
    public override string Name { get; } = "ban";

    /// <inheritdoc />
    public override string Description { get; } = "Bans a player from the server.";

    /// <inheritdoc />
    public override string Usage { get; } = "ban <Player (by name, steamID or playerID)> <Optional string Reason> <Optional duration (Xh or Xd)>";

    /// <inheritdoc />
    public bool Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public bool Validate(string[] args)
    {
        return args.Length is >= 1 and <= 3 &&
               (PlayerUtils.TryFindPlayer(args[0], out _) || ulong.TryParse(args[0], out _));
    }

    /// <inheritdoc />
    public bool Execute(Player player, string[] args, out string? response)
    {
        var target = args[0];
        PlayerUtils.TryFindPlayer(target, out var targetPlayer);
        if (targetPlayer != player) return Behaviour(args, false, out response);
        response = "You cannot ban yourself!";
        return false;
    }

    /// <inheritdoc />
    public bool Execute(string[] args, out string? response) => Behaviour(args, true, out response);
    
    
    private bool Behaviour(string[] args, bool comesFromIpc, out string? response ){
        var target = args[0];
        var reason = args.Length > 1 ? args[1] : "Unknown reason";
        string? duration = null;
        if (args.Length == 3)
        {
            duration = args[2];
        }
        ulong banSteamID;
        if (ulong.TryParse(target, out var targetID) && targetID > (ulong)Globals.DedicatedServerManagerInstance.Config.MaxPlayers)
        {
            banSteamID = targetID;
            response = $"Banned player with steamID {banSteamID} for reason {reason}";
            if (PlayerUtils.TryFindPlayerBySteamId(banSteamID, out var targetPlayer))
            {
                Globals.NetworkManagerNuclearOptionInstance.KickPlayerAsync(targetPlayer);
            }
        }
        else
        {
            var rs = PlayerUtils.TryFindPlayer(target, out var player);
            if (!rs)
                throw new VerificationException(
                    $"Could not find player {target}: validation was not called properly.");
            banSteamID = player!.SteamID;
            Globals.NetworkManagerNuclearOptionInstance.KickPlayerAsync(player);
            response = $"Banned player {player.PlayerName} for reason {reason}";
        }

        if (duration is not null)
        {
            response += $" for {duration}.";
        }

        PlayerUtils.BanPlayer(banSteamID, reason, duration, comesFromIpc);
        if (!GwServerPlugin.FamilySharingBorrowers.TryGetValue(banSteamID, out var ownerSteamID)) return true;
        if (ownerSteamID != banSteamID)
        {
            PlayerUtils.BanPlayer(ownerSteamID, $"Owner of family shared banned account. Child banned for {reason}",
                duration, comesFromIpc);
            response += $"\tBanned Owner with steamID {ownerSteamID} as well";
        }
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
}