using System;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.Chat;
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
        new("player", ChatArgumentType.Player)
    ];

    /// <inheritdoc />
    public override UniTask<ChatCommandResult> Execute(string[] args)
    {
        if (!ChatHelper.TryParseSteamId(args[0], out var targetSteamID))
            return UniTask.FromResult(ChatCommandResult.ClientError($"Invalid User: {args[0]}"));
        _ = Enum.TryParse<PermissionLevel>(args[1], out var level);
        PluginConfig.SetPermissionLevel(targetSteamID.m_SteamID, level);
        if (PlayerHandle.TryGet(targetSteamID.m_SteamID, out var playerHandle))
            playerHandle.SetPermissionLevel(PermissionLevelUtils.ProtoToShockfrontPermissionLevel(level));

        return UniTask.FromResult(ChatCommandResult.Ok($"Successfully set permission level to {level}"));
    }

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Owner;
}