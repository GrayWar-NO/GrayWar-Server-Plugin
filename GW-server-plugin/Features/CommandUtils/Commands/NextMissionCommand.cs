using BepInEx.Configuration;
using GW_server_plugin.Enums;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command for switching the currently active mission on the server.
/// </summary>
/// <param name="config"></param>
public class NextMissionCommand(ConfigFile config): PermissionConfigurableCommand(config)
{
    /// <inheritdoc />
    public override string Name { get; } = "nextmission";

    /// <inheritdoc />
    public override string Description { get; } = "Starts the next mission, or a selected mission from index";

    /// <inheritdoc />
    public override string Usage { get; } = "nextmission <int MissionIndex?> (omitting mission index will use the mission rotation instead)";

    /// <inheritdoc />
    public override bool Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public override bool Validate(string[] args)
    {
        if (args.Length > 1) return false;
        if (args.Length == 0) return true;
        return int.TryParse(args[0], out _);
    }

    /// <inheritdoc />
    public override bool Execute(Player player, string[] args, out string? response) => Execute(args, out response);

    /// <inheritdoc />
    public override bool Execute(string[] args, out string? response)
    {
        response = "Called NextMission asynchronously, no result is available.";
        if (args.Length == 0)
        {
            _ = MissionService.StartNextMission();
            return true;
        }
        var missionIndex = int.Parse(args[0]);
        var missionOption = MissionService.GetMissionOptionByIndex(missionIndex);
        if (missionOption == null)
        {
            response = "Mission not found.";
            return false;
        }
        _ = MissionService.StartMission(missionOption.Value);
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
}