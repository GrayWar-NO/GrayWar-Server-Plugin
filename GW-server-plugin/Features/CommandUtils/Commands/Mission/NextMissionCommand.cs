using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands.Mission;

/// <summary>
///     Command for switching the currently active mission on the server.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class NextMissionCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
    
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
    public async UniTask<(bool success, string? response)> Execute(string[] args)
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
    
    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);
    
    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Execute(args);
}