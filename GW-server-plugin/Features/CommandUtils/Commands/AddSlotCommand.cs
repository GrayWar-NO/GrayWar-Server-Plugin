using BepInEx.Configuration;
using GW_server_plugin.Enums;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to add a slot to the server.
/// </summary>
/// <param name="config"></param>
public class AddSlotCommand(ConfigFile config): PermissionConfigurableCommand(config), IConsoleCommand
{
    /// <inheritdoc />
    public override string Name =>  "addslot";

    /// <inheritdoc />
    public override string Description => "Adds a slot to the server";

    /// <inheritdoc />
    public override string Usage => "addslot (takes no arguments)";

    /// <inheritdoc />
    public bool Validate(string[] args)
    {
        return args.Length == 0;
    }

    /// <inheritdoc />
    public bool Execute(string[] args, out string? response)
    {
        StaffSlotService.AddStaffSlot();
        response = "Successfully added a slot to the server.";
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}