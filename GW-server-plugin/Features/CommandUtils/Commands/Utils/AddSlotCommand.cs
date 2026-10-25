using BepInEx.Configuration;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Utils;

/// <summary>
/// Command to add a slot to the server.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class AddSlotCommand(ConfigFile config): ConfigurableCommand(config), IConsoleCommand
{
    /// <inheritdoc />
    public override string Name =>  "addslot";

    /// <inheritdoc />
    public override string Description => "Adds a slot to the server";

    /// <inheritdoc />
    public ChatCommandResult Execute(string[] args)
    {
        StaffSlotService.AddStaffSlot();
        return ChatCommandResult.Ok("Successfully added a slot to the server.");
    }

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}