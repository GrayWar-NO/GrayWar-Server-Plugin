using BepInEx.Configuration;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Utils;

/// <summary>
/// Command to remove a slot from the server.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class RemoveSlotCommand(ConfigFile config): ConfigurableCommand(config), IConsoleCommand
{
    /// <inheritdoc />
    public override string Name =>  "rmslot";

    /// <inheritdoc />
    public override string Description => "Removes a slot from the server";
    
    /// <inheritdoc />
    public ChatCommandResult Execute(string[] args)
    {
        var result = StaffSlotService.RemoveStaffSlot();
        if (!result)
        {
            return ChatCommandResult.ClientError("Failed to remove staff slot: No staff slots left to remove.");
        }
        return ChatCommandResult.Ok("Successfully removed a slot from the server.");
    }

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}