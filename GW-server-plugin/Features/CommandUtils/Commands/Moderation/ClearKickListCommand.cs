using System;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Moderation;

/// <summary>
///     Clear the integrated kick list.    
/// </summary>
[AutoCommand]
public class ClearKickListCommand(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string Name => "clearkicklist";

    /// <inheritdoc />
    public override string Description => "Clears the integrated kick list. Manual or vote clears only the manual kicks or the votekicks. Defaults to both.";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        ChatCommandArgument.FromChoices("list", ["manual", "vote"], true)
    ];
    
    /// <inheritdoc />
    public override ChatCommandResult Execute(string[] args)
    {
        var mode = args.Length > 0 ? args[0] : null;

        if (!StringComparer.OrdinalIgnoreCase.Equals(mode, "vote"))
            Globals.NetworkManagerNuclearOptionInstance.Authenticator.KickList.Clear();

        if (!StringComparer.OrdinalIgnoreCase.Equals(mode, "manual"))
            Globals.NetworkManagerNuclearOptionInstance.Authenticator.MissionKickList.Clear();
        return ChatCommandResult.Ok($"{mode}Kick list cleared successfully!");
    }
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Admin;
}