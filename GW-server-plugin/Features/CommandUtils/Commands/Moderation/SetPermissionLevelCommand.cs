using System;
using BepInEx.Configuration;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Moderation;

/// <summary>
/// Command for setting the permission level of a player.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class SetPermissionLevelCommand(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string Name => "setpermissionlevel";

    /// <inheritdoc />
    public override string Description => "Set the permission level of a player.";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("player (steamID)", ChatArgumentType.Text), // TODO wait for ulong type
    ];

    /// <inheritdoc />
    public override ChatCommandResult Execute(string[] args)
    {
        var targetSteamID = ulong.Parse(args[0]);
        _ = Enum.TryParse<PermissionLevel>(args[1], out var level);
        PluginConfig.SetPermissionLevel(targetSteamID, level);
        return ChatCommandResult.Ok($"Successfully set permission level to {level}");
    }

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Owner;
}