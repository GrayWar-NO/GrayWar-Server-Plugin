using System;
using System.Linq;
using System.Security;
using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Features.IPC.Packets;
using GW_server_plugin.Helpers;
using Newtonsoft.Json;
using NuclearOption.Chat;
using NuclearOption.DedicatedServer;
using NuclearOption.Networking;
using Steamworks;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to ban a player.
/// </summary>
/// <param name="config"></param>
public class WarnCommand(ConfigFile config): PermissionConfigurableCommand(config)
{
    /// <inheritdoc />
    public override string Name { get; } = "warn";

    /// <inheritdoc />
    public override string Description { get; } = "Warns a player.";

    /// <inheritdoc />
    public override string Usage { get; } = "warn <Player (by name, steamID or playerID)> <Reason>";

    /// <inheritdoc />
    public override bool Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public override bool Validate(string[] args)
    {
        return args.Length >= 1 && (PlayerUtils.TryFindPlayer(args[0], out _) || ulong.TryParse(args[0], out _));
    }

    /// <inheritdoc />
    public override bool Execute(Player player, string[] args, out string? response)
    {
        var target = args[0];
        PlayerUtils.TryFindPlayer(target, out var targetPlayer);
        if (targetPlayer != player) return Execute(args, out response);
        response = "You cannot warn yourself!";
        return false;
    }

    /// <inheritdoc />
    public override bool Execute(string[] args, out string? response)
    {
        var target = args[0];
        var reason = string.Join(" ", args.Skip(1)); 
        ulong warnSteamID;
        if (ulong.TryParse(target, out var targetID) && targetID >= (ulong)Globals.DedicatedServerManagerInstance.Config.MaxPlayers)
        {
            warnSteamID = targetID;
            response = $"Warned player with steamID {warnSteamID} for reason {reason}";
        }
        else
        {
            var rs = PlayerUtils.TryFindPlayer(target, out var player);
            if (!rs)
                throw new VerificationException(
                    $"Could not find player {target}: validation was not called properly.");
            warnSteamID = player!.SteamID;
            response = $"Warned player {player.PlayerName} for reason {reason}";
            ChatService.SendPrivateChatMessage($"You have been warned for {reason}", player);
        }

        var warnLogPacket = new LogEntryPacket
        {
            LogText = $"{warnSteamID}:{reason}",
            Channel = LogChannel.Warn
        };
        GwServerPlugin.LoggingOutBox.Add(warnLogPacket);
        return GwServerPlugin.WarnService.AddWarn(warnSteamID);
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
}