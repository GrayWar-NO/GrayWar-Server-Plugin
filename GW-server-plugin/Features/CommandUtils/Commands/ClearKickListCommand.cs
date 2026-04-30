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
    public override string Name { get; } = "clearkicklist";

    /// <inheritdoc />
    public override string Description { get; } = "Clears the integrated kick list";

    /// <inheritdoc />
    public override string Usage { get; } = "clearkicklist (takes no arguments)";

    /// <inheritdoc />
    public bool Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public bool Validate(string[] args)
    {
        return args.Length == 0;
    }

    /// <inheritdoc />
    public bool Execute(Player player, string[] args, out string? response) => Execute(args, out response);

    /// <inheritdoc />
    public bool Execute(string[] args, out string? response)
    {
        Globals.NetworkManagerNuclearOptionInstance.Authenticator.KickList.Clear();
        response = "Kick list cleared successfully!";
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Admin;
}