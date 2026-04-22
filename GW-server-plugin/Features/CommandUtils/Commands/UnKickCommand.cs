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
public class UnKickCommand(ConfigFile config): PermissionConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string Name { get; } = "unkick";

    /// <inheritdoc />
    public override string Description { get; } = "Unkicks a player from the  server.";

    /// <inheritdoc />
    public override string Usage { get; } = "unkick <player (by steamID only)>";

    /// <inheritdoc />
    public bool Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public bool Validate(string[] args)
    {
        return args.Length == 1 && ulong.TryParse(args[0], out _);
    }

    /// <inheritdoc />
    public bool Execute(Player player, string[] args, out string? response) => Execute(args, out response);

    /// <inheritdoc />
    public bool Execute(string[] args, out string? response)
    {
        UnKickPlayer(ulong.Parse(args[0]));
        response = $"Unkicked player with steamID {args[0]}";
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;

    private static void UnKickPlayer(ulong steamID)
    {
        Globals.NetworkManagerNuclearOptionInstance.Authenticator.RemoveKicked(new CSteamID(steamID));
        var kickLogPacket = new LogEntryPacket
        {
            LogText = $"0:{steamID}:",
            Channel = LogChannel.Kick
        };
        GwServerPlugin.LoggingOutBox.Add(kickLogPacket);
    }
    
}