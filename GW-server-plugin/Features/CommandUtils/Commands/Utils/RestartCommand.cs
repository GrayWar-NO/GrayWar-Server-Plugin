using System;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Utils;

/// <summary>
/// Donate a specified sum in millions to a player
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class Restart(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string Name => "restart";
    
    /// <inheritdoc />
    public override string Description => "restart server after mission ends";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        ChatCommandArgument.FromChoices("option", ["force", "cancel"], true),
        new("reason", ChatArgumentType.Text, true)
    ];
    
    /// <inheritdoc />
    public override ChatCommandResult Execute(string[] args)
    {
        try
        {
            if (args.Length >= 1 && args[0] == "cancel")
            {
                var cancelReason = args.Length >= 2 ? string.Join(" ", args, 1, args.Length - 1) : null;
                RestartService.CancelScheduledRestart(cancelReason);
                return ChatCommandResult.Ok("Restart canceled");
            }
            
            // Force restart if there are no players
            var playerCount = PlayerUtils.GetPlayerCount();
            if (playerCount == 0 || (args.Length == 1 && (args[0] == "force" || args[0] == "f")))
                return ChatCommandResult.Ok("Server restarting...");
            
            var reason = (args.Length != 0) ? string.Join(" ", args) : null;
            RestartService.ScheduleRestart(reason);
            return ChatCommandResult.Ok("Server has been scheduled to restart after mission");
        }
        catch (Exception e)
        {
            GwServerPlugin.Logger.LogError(e);
            return ChatCommandResult.Error(1,e.Message);
        }
    }
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}