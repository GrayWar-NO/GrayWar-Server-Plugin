using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
///     Clear the integrated kick list.    
/// </summary>
public class ClearKickListCommand(ConfigFile config) : PermissionConfigurableCommand(config)
{
    /// <inheritdoc />
    public override string Name { get; } = "clearkicklist";

    /// <inheritdoc />
    public override string Description { get; } = "Clears the integrated kick list";

    /// <inheritdoc />
    public override string Usage { get; } = "clearkicklist (takes no arguments)";

    /// <inheritdoc />
    public override bool Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public override bool Validate(string[] args)
    {
        return args.Length == 0;
    }

    /// <inheritdoc />
    public override bool Execute(Player player, string[] args, out string? response) => Execute(args, out response);

    /// <inheritdoc />
    public override bool Execute(string[] args, out string? response)
    {
        Globals.NetworkManagerNuclearOptionInstance.Authenticator.ClearKickList();
        response = "Kick list cleared successfully!";
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Admin;
}