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
    public override async UniTask<ChatCommandResult> Execute(string[] args)
    {
        if (GameManager.gameResolution != GameResolution.Ongoing)
            return ChatCommandResult.ClientError("Cannot start next mission: current mission has ended");
        
        if (args.Length == 0)
        {
            var (valid, _) = await MissionService.StartNextMission();
            return valid
                ? ChatCommandResult.Ok("Next mission started successfully.")
                : ChatCommandResult.ClientError("Failed to start next mission.");
        }
        
        var missionIndex = int.Parse(args[0]);
        var missionOption = MissionService.GetMissionOptionByIndex(missionIndex);
        if (missionOption == null) return ChatCommandResult.ClientError("Mission not found.");
        
        {
            var (valid, mission) = await MissionService.StartMission(missionOption.Value);
            return valid
                ? ChatCommandResult.Ok($"Started mission {mission!.Name} successfully.")
                : ChatCommandResult.ClientError("Failed to start next mission.");
        }
    }
    
    /// <inheritdoc />
    public override string Name => "nextmission";
    
    /// <inheritdoc />
    public override string Description => "Starts the next mission, or a selected mission from index";
}