using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Mission;

/// <summary>
///     Command for switching the currently active mission on the server.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class NextMissionCommand(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("MissionID", ChatArgumentType.Text, true)
    ];
    
    /// <inheritdoc />
    public override async ChatCommandResult Execute(string[] args) // TODO waiting for async
    {
        if (GameManager.gameResolution != GameResolution.Ongoing)
            return (false, "Cannot start next mission: current mission has ended");
        
        if (args.Length == 0)
        {
            var (valid, _) = await MissionService.StartNextMission();
            return valid ? (true, "Next mission started successfully.") : (false, "Failed to start next mission.");
        }
        
        var missionIndex = int.Parse(args[0]);
        var missionOption = MissionService.GetMissionOptionByIndex(missionIndex);
        if (missionOption == null) return (false, "Mission not found.");
        
        {
            var (valid, mission) = await MissionService.StartMission(missionOption.Value);
            return valid
                ? (true, $"Started mission {mission!.Name} successfully.")
                : (false, "Failed to start next mission.");
        }
    }
    
    /// <inheritdoc />
    public override string Name => "nextmission";
    
    /// <inheritdoc />
    public override string Description => "Starts the next mission, or a selected mission from index";
    
    /// <inheritdoc />
    public override string Usage =>
        "nextmission <int MissionIndex?> (omitting mission index will use the mission rotation instead)";
}