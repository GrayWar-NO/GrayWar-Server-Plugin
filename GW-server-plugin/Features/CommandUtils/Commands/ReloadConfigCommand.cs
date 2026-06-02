using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command for reloading config files.
/// </summary>
/// <param name="config"></param>
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
    public bool Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public bool Validate(string[] args)
    {
        return args.Length == 1 && AllowedValues.Contains(args[0]);
    }

    /// <inheritdoc />
    public bool Execute(Player player, string[] args, out string? response) => Execute(args, out response);

    /// <inheritdoc />
    public bool Execute(string[] args, out string? response)
    {
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

        return true;
    }
}