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
public class UnbanCommand(ConfigFile config): PermissionConfigurableCommand(config)
{
    /// <inheritdoc />
    public override string Name { get; } = "unban";

    /// <inheritdoc />
    public override string Description { get; } = "Unbans a player from the server.";

    /// <inheritdoc />
    public override string Usage { get; } = "unban <Player (by name, steamID or playerID)>";

    /// <inheritdoc />
    public override bool Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public override bool Validate(string[] args)
    {
        return args.Length == 1 && (PlayerUtils.TryFindPlayer(args[0], out _) || ulong.TryParse(args[0], out _));
    }

    /// <inheritdoc />
    public override bool Execute(Player player, string[] args, out string? response) => Execute(args, out response);

    /// <inheritdoc />
    public override bool Execute(string[] args, out string? response)
    {
        var target = args[0];
        ulong banSteamID;
        if (ulong.TryParse(target, out var targetID) && targetID >= (ulong)Globals.DedicatedServerManagerInstance.Config.MaxPlayers)
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
        var banLogPacket = new LogEntryPacket
        {
            LogText = $"0:{banSteamID}:",
            Channel = LogChannel.Ban
        };
        GwServerPlugin.SocketOutBox.Add(JsonConvert.SerializeObject(banLogPacket));
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
}