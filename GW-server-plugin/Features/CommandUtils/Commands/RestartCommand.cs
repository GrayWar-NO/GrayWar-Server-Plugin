using System;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
using GW_server_plugin.Features;
using GW_server_plugin.Features.CommandUtils;
using NuclearOption.Networking;

namespace GW_server_plugin.CritzOS;

/// <summary>
/// Donate a specified sum in millions to a player
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class Restart(ConfigFile config): PermissionConfigurableCommand(config), IGameCommand, IConsoleCommand
{

    /// <inheritdoc />
    public override string Name => "restart";

    /// <inheritdoc />
    public override string Description => "restart server";

    /// <inheritdoc />
    public override string Usage => "/restart";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => UniTask.FromResult(args.Length == 0);

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args) => UniTask.FromResult(args.Length == 0);


    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Execute(args);
    
    
    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        try
        {
            return UniTask.FromResult<(bool, string?)>((RestartService.Restart(), $"Server restarting..."));
        }
        catch (Exception e)
        {
            GwServerPlugin.Logger.LogError(e);
            return UniTask.FromResult<(bool, string?)>((false, e.Message));
        }
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;

}