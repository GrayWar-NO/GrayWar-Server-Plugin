using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;

namespace GW_server_plugin.Features.CommandUtils.Commands;

using BepInEx.Configuration;

/// <summary>
/// Command to remove a slot from the server.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class RemoveSlotCommand(ConfigFile config): PermissionConfigurableCommand(config), IConsoleCommand
{
    /// <inheritdoc />
    public override string Name =>  "rmslot";

    /// <inheritdoc />
    public override string Description => "Removes a slot from the server";

    /// <inheritdoc />
    public override string Usage => "rmslot (takes no arguments)";
    
    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(args.Length == 0);
    }
    
    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var result = StaffSlotService.RemoveStaffSlot();
        if (!result)
        {
            return UniTask.FromResult<(bool, string?)>((false, "Failed to remove staff slot: No staff slots left to remove."));
        }
        return UniTask.FromResult<(bool, string?)>((true, "Successfully removed a slot from the server."));
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}