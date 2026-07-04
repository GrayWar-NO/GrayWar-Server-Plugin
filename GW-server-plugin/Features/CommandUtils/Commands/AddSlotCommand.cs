using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to add a slot to the server.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class AddSlotCommand(ConfigFile config): PermissionConfigurableCommand(config), IConsoleCommand
{
    /// <inheritdoc />
    public override string Name =>  "addslot";

    /// <inheritdoc />
    public override string Description => "Adds a slot to the server";

    /// <inheritdoc />
    public override string Usage => "addslot (takes no arguments)";

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(args.Length == 0);
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        StaffSlotService.AddStaffSlot();
        return UniTask.FromResult<(bool, string?)>((true, "Successfully added a slot to the server."));
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}