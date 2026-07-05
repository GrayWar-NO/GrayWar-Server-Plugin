using System;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
///     Clear the integrated kick list.    
/// </summary>
[AutoCommand]
public class ClearKickListCommand(ConfigFile config) : PermissionConfigurableCommand(config), IConsoleCommand, IGameCommand
{
    /// <inheritdoc />
    public override string Name => "clearkicklist";

    /// <inheritdoc />
    public override string Description => "Clears the integrated kick list. Manual or vote clears only the manual kicks or the votekicks. Defaults to both.";

    /// <inheritdoc />
    public override string Usage => "clearkicklist <optional 'manual' or 'vote'>";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(args.Length == 0 ||
                                  (args.Length == 1 &&
                                   (StringComparer.OrdinalIgnoreCase.Equals(args[0], "manual") ||
                                    StringComparer.OrdinalIgnoreCase.Equals(args[0], "vote"))));
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Execute(args);

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var mode = args.Length > 0 ? args[0] : null;

        if (!StringComparer.OrdinalIgnoreCase.Equals(mode, "vote"))
            Globals.NetworkManagerNuclearOptionInstance.Authenticator.KickList.Clear();

        if (!StringComparer.OrdinalIgnoreCase.Equals(mode, "manual"))
            Globals.NetworkManagerNuclearOptionInstance.Authenticator.MissionKickList.Clear();
        return UniTask.FromResult<(bool, string?)>((true, $"{mode}Kick list cleared successfully!"));
    }
    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Admin;
}