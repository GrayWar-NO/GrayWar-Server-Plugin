using System;
using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
///     Clear the integrated kick list.    
/// </summary>
public class ClearKickListCommand(ConfigFile config) : PermissionConfigurableCommand(config), IConsoleCommand, IGameCommand
{
    /// <inheritdoc />
    public override string Name => "clearkicklist";

    /// <inheritdoc />
    public override string Description => "Clears the integrated kick list. Manual or vote clears only the manual kicks or the votekicks. Defaults to both.";

    /// <inheritdoc />
    public override string Usage => "clearkicklist <optional 'manual' or 'vote'>";

    /// <inheritdoc />
    public bool Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public bool Validate(string[] args)
    {
        return args.Length == 0 ||
               (args.Length == 1 &&
                (StringComparer.OrdinalIgnoreCase.Equals(args[0], "manual") ||
                 StringComparer.OrdinalIgnoreCase.Equals(args[0], "vote")));
    }

    /// <inheritdoc />
    public bool Execute(Player player, string[] args, out string? response) => Execute(args, out response);

    /// <inheritdoc />
    public bool Execute(string[] args, out string? response)
    {
        var mode = args.Length > 0 ? args[0] : null;

        if (!StringComparer.OrdinalIgnoreCase.Equals(mode, "vote"))
            Globals.NetworkManagerNuclearOptionInstance.Authenticator.KickList.Clear();

        if (!StringComparer.OrdinalIgnoreCase.Equals(mode, "manual"))
            Globals.NetworkManagerNuclearOptionInstance.Authenticator.MissionKickList.Clear();

        response = $"{mode}Kick list cleared successfully!";
        return true;
    }
    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Admin;
}