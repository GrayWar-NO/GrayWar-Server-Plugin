using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command for switching the currently active mission on the server.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class NextMissionCommand(ConfigFile config): PermissionConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string Name => "nextmission";

    /// <inheritdoc />
    public override string Description => "Starts the next mission, or a selected mission from index";

    /// <inheritdoc />
    public override string Usage => "nextmission <int MissionIndex?> (omitting mission index will use the mission rotation instead)";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(args.Length switch
        {
            > 1 => false,
            0 => true,
            _ => int.TryParse(args[0], out _)
        });
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Execute(args);

    /// <inheritdoc />
    public async UniTask<(bool success, string? response)> Execute(string[] args)
    {
        if (args.Length == 0)
        {
            GwServerPlugin.MissionVote.Inhibit("Switching mission."); // Inhibit is lifted at mission load.
            if (await MissionService.StartNextMission())
                return (true, "Next mission started successfully.");
            return (false, "Failed to start next mission.");
        }
        var missionIndex = int.Parse(args[0]);
        var missionOption = MissionService.GetMissionOptionByIndex(missionIndex);
        if (missionOption == null)
        {
            return (false,  "Mission not found.");
        }
        GwServerPlugin.MissionVote.Inhibit("Switching mission."); // Inhibit is lifted at mission load.
        if (await MissionService.StartMission(missionOption.Value))
            return (true, $"Started mission {missionOption.Value.Key.Name} successfully.");
        return (false, "Failed to start next mission.");
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
}