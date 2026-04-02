namespace GW_server_plugin.Features.CommandUtils.Commands;

using Enums;
using BepInEx.Configuration;
using NuclearOption.Networking;

/// <summary>
/// Command to remove a slot from the server.
/// </summary>
/// <param name="config"></param>
public class RemoveSlotCommand(ConfigFile config): PermissionConfigurableCommand(config)
{
    /// <inheritdoc />
    public override string Name =>  "rmslot";

    /// <inheritdoc />
    public override string Description => "Removes a slot from the server";

    /// <inheritdoc />
    public override string Usage => "rmslot (takes no arguments)";

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
        var r = StaffSlotService.RemoveStaffSlot();
        if (!r)
        {
            response = "Failed to remove staff slot: No staff slots left to remove.";
            return false;
        }
        response = "Successfully removed a slot from the server.";
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}