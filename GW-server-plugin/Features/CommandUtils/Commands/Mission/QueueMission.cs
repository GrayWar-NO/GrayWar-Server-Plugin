using BepInEx.Configuration;
using GW_server_plugin.Helpers;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Mission;

/// <summary>
///     Command for queuing a mission to be loaded next on the server.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class QueueMission(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string Name => "queue";
    
    /// <inheritdoc />
    public override string Description => "Queue a mission to be loaded next on the server.";
    
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("MissionID", ChatArgumentType.Text) // TODO waiting for INT type
    ];
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
    
    /// <inheritdoc />
    public override ChatCommandResult Execute(string[] args)
    {
        var missionIndex = int.Parse(args[0]);
        var missionOption = MissionService.GetMissionOptionByIndex(missionIndex);
        if (missionOption == null) return ChatCommandResult.ClientError("Mission not found.");
        {
            Globals.DedicatedServerManagerInstance.missionRotation.OverrideNext(missionOption.Value);
#pragma warning disable CS8619
            return ChatCommandResult.Ok($"Queued mission {missionOption.Value.Key.Name} successfully.");
#pragma warning restore CS8619
        }

    }
}