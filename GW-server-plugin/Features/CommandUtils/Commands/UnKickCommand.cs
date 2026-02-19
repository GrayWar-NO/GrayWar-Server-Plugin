using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Features.IPC.Packets;
using GW_server_plugin.Helpers;
using Newtonsoft.Json;
using NuclearOption.Networking;
using Steamworks;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to kick a player from the server
/// </summary>
/// <param name="config"></param>
public class UnKickCommand(ConfigFile config): PermissionConfigurableCommand(config)
{
    /// <inheritdoc />
    public override string Name { get; } = "unkick";

    /// <inheritdoc />
    public override string Description { get; } = "Unkicks a player from the  server.";

    /// <inheritdoc />
    public override string Usage { get; } = "unkick <player (by name, steamID or ID tag)>";

    /// <inheritdoc />
    public override bool Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public override bool Validate(string[] args)
    {
        return args.Length >= 1 && PlayerUtils.TryFindPlayer(string.Join(" ", args), out _);
    }

    /// <inheritdoc />
    public override bool Execute(Player player, string[] args, out string? response) => Execute(args, out response);

    /// <inheritdoc />
    public override bool Execute(string[] args, out string? response)
    {
        var target = string.Join(" ", args);
        if (PlayerUtils.TryFindPlayer(target, out var targetPlayer))
        {
            UnKickPlayer(targetPlayer!);
            response = $"{targetPlayer!.PlayerName} has been unkicked!";
            return true;
        }
        response = $"{target} is not a valid player!";
        return false;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;

    private static void UnKickPlayer(Player player)
    {
        Globals.NetworkManagerNuclearOptionInstance.Authenticator.RemoveKicked(new CSteamID(player.SteamID));
        var kickLogPacket = new LogEntryPacket
        {
            LogText = $"0:{player.SteamID}:",
            Channel = LogChannel.Kick
        };
        GwServerPlugin.SocketOutBox.Enqueue(JsonConvert.SerializeObject(kickLogPacket));
    }
    
}