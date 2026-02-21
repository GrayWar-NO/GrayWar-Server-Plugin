using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to list missions on the server
/// </summary>
/// <param name="config"></param>
public class ListMissionsCommand(ConfigFile config): PermissionConfigurableCommand(config)
{
    /// <inheritdoc />
    public override string Name { get; } = "missions";

    /// <inheritdoc />
    public override string Description { get; } = "List all currently available missions";

    /// <inheritdoc />
    public override string Usage { get; } = "missions (takes no arguments)";

    /// <inheritdoc />
    public override bool Validate(Player player, string[] args)
    {
        return Validate(args);
    }

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
        var missions = MissionService.GetAllAvailableMissionOptions();
        if (missions.Length == 0)
        {
            response = "No available missions";
            return true;
        }

        response = "Available missions:\n";
        for (var i = 0; i < missions.Length; i++)
        {
            response += $"[{i}] {missions[i].Key.Name}\n";
        }
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Everyone;
}