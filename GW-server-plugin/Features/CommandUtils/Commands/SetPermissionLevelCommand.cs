using System;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command for setting the permission level of a player.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class SetPermissionLevelCommand(ConfigFile config) : PermissionConfigurableCommand(config), IGameCommand
{
    /// <inheritdoc />
    public override string Name => "setpermissionlevel";

    /// <inheritdoc />
    public override string Description => "Set the permission level of a player.";

    /// <inheritdoc />
    public override string Usage => "setpermissionlevel <player (by steamID)> <permission_level>";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args)
    {
        return UniTask.FromResult(args.Length == 2 &&
                                  Enum.TryParse<PermissionLevel>(args[1], out _) && 
                                  ulong.TryParse(args[0], out _));
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        var targetSteamID = ulong.Parse(args[0]);
        _ = Enum.TryParse<PermissionLevel>(args[1], out var level);
        PluginConfig.SetPermissionLevel(targetSteamID, level);
        return UniTask.FromResult<(bool, string?)>((true, $"Successfully set permission level to {level}"));
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Owner;
}