using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command for setting the permission level of a player.
/// </summary>
/// <param name="config"></param>
public class SetPermissionLevelCommand(ConfigFile config) : PermissionConfigurableCommand(config), IGameCommand
{
    /// <inheritdoc />
    public override string Name { get; } = "setpermissionlevel";

    /// <inheritdoc />
    public override string Description { get; } = "Set the permission level of a player.";

    /// <inheritdoc />
    public override string Usage { get; } = "setpermissionlevel <player (by steamID)> <permission_level>";

    /// <inheritdoc />
    public bool Validate(Player player, string[] args) 
    {
        return args.Length == 2 &&
               PermissionLevelUtils.TryParsePermissionLevel(args[1], out _) &&
               ulong.TryParse(args[0], out _);
    }

    /// <inheritdoc />
    public bool Execute(Player player, string[] args, out string? response)
    {
        var targetSteamID = ulong.Parse(args[0]);
        _ = PermissionLevelUtils.TryParsePermissionLevel(args[1], out var level);
        PluginConfig.SetPermissionLevel(targetSteamID, level);
        response = $"Successfully set permission level to {level}";
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Owner;
}