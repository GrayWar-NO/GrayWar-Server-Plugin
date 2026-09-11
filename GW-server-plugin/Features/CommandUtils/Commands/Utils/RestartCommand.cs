using System;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands.Utils;

/// <summary>
/// Donate a specified sum in millions to a player
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class Restart(ConfigFile config) : ConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string OutputName => "restart";

    /// <inheritdoc />
    public override string Description => "restart server after mission ends";

    /// <inheritdoc />
    public override string Usage => $"restart <(optional) [f]orce OR [c]ancel> <(optional) reason>. e.g: /restart c server update";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(true);
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Execute(args);


    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        try
        {
            if (args.Length >= 1 && (args[0] == "cancel" || args[0] == "c"))
            {
                string? cancelReason = (args.Length >= 2) ? string.Join(" ", args, 1, args.Length - 1) : null;
                RestartService.CancelScheduledRestart(cancelReason);
                return UniTask.FromResult<(bool, string?)>((true, "Restart canceled"));
            }
            
            // Force restart if there are no players
            var playerCount = PlayerUtils.GetPlayerCount();
            if (playerCount == 0 || (args.Length == 1 && (args[0] == "force" || args[0] == "f")))
                return UniTask.FromResult<(bool, string?)>((RestartService.Restart(), "Server restarting..."));

            var reason = (args.Length != 0) ? string.Join(" ", args) : null;
            RestartService.ScheduleRestart(reason);
            return UniTask.FromResult<(bool, string?)>((true, "Server has been scheduled to restart after mission"));
        }
        catch (Exception e)
        {
            GwServerPlugin.Logger.LogError(e);
            return UniTask.FromResult<(bool, string?)>((false, e.Message));
        }
    }

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}