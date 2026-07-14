using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command for reloading config files.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class ReloadConfigCommand(ConfigFile config)
    : PermissionConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    private static readonly HashSet<string> AllowedValues =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "server",
            "bepinex",
            "both"
        };

    /// <inheritdoc />
    public override string Name => "reload";

    /// <inheritdoc />
    public override string Description => "Reload the plugin config, the dedicated server config, or both.";

    /// <inheritdoc />
    public override string Usage => "/reload <bepinex, server or both (keywords)>";

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Admin;

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(args.Length == 1 && AllowedValues.Contains(args[0]));
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Execute(args);

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        string? response;
        switch (args[0].ToLowerInvariant())
        {
            case "bepinex":
                GwServerPlugin.Instance.Config.Reload();
                response = "Reloaded BepInEx config successfully.";
                break;
            case "server":
                Globals.DedicatedServerManagerInstance.ReloadConfig(null, null);
                response = "Reloaded server config successfully!";
                break;
            case "both":
                Globals.DedicatedServerManagerInstance.ReloadConfig(null, null);
                GwServerPlugin.Instance.Config.Reload();
                response = "Reloaded both configs successfully!";
                break;
            default:
                response = $"Unknown config source '{args[0]}'. Validation was not called correctly.";
                break;
        }

        return UniTask.FromResult<(bool, string?)>((true, response));
    }
}