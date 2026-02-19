using System.Linq;
using System.Security;
using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Features.IPC.Packets;
using GW_server_plugin.Helpers;
using Newtonsoft.Json;
using NuclearOption.DedicatedServer;
using NuclearOption.Networking;
using Steamworks;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to ban a player.
/// </summary>
/// <param name="config"></param>
public class BanCommand(ConfigFile config): PermissionConfigurableCommand(config)
{
    /// <inheritdoc />
    public override string Name { get; } = "ban";

    /// <inheritdoc />
    public override string Description { get; } = "Bans a player from the server.";

    /// <inheritdoc />
    public override string Usage { get; } = "ban <Player (by name, steamID or playerID)> <Reason>";

    /// <inheritdoc />
    public override bool Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public override bool Validate(string[] args)
    {
        return args.Length >= 2 && (PlayerUtils.TryFindPlayer(args[0], out _) || ulong.TryParse(args[0], out _));
    }

    /// <inheritdoc />
    public override bool Execute(Player player, string[] args, out string? response)
    {
        var target = args[0];
        PlayerUtils.TryFindPlayer(target, out var targetPlayer);
        if (targetPlayer != player) return Execute(args, out response);
        response = "You cannot ban yourself!";
        return false;
    }

    /// <inheritdoc />
    public override bool Execute(string[] args, out string? response)
    {
        var target = args[0];
        var reason = string.Join(" ", args.Skip(1).ToArray()); 
        ulong banSteamID;
        if (ulong.TryParse(target, out var targetID) && targetID >= (ulong)Globals.DedicatedServerManagerInstance.Config.MaxPlayers)
        {
            banSteamID = targetID;
            response = $"Banned player with steamID {banSteamID} for reason {reason}";
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

        
        var ownerSteamID = GwServerPlugin.FamilySharingBorrowers[banSteamID];
        PlayerUtils.BanPlayer(banSteamID, reason);
        if (ownerSteamID != banSteamID)
        {
            PlayerUtils.BanPlayer(ownerSteamID, $"Owner of family shared banned account. Child banned for {reason}");
            response += $"\tBanned Owner with steamID {ownerSteamID} as well";
        }
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
}