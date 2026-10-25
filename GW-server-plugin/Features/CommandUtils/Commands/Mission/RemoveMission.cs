using BepInEx.Configuration;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Mission;

/// <summary>
///     Command to remove a mission from rotation.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class RemoveMission(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string Name => "rmmission";
    
    /// <inheritdoc />
    public override string Description => "Removes a mission from rotation";
    
    /// <inheritdoc />
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("missionID", ChatArgumentType.Text), // TODO waiting for INT type
        new("save", ChatArgumentType.Text, true)
    ];
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
    
    
    /// <inheritdoc />
    public override ChatCommandResult Execute(string[] args)
    {
        var missionID = int.Parse(args[0]);
        bool? save = null;
        if (args.Length == 2)
            save = bool.Parse(args[1]);
        
        var missions = MissionService.GetAllAvailableMissionOptions();
        
        if (missionID >= missions.Length)
            return ChatCommandResult.ClientError(
                $"{missionID} is not a valid missionID. Max is {missions.Length - 1}. Use {PluginConfig.CommandPrefixChar}missions to get list.");
        
        var name = missions[missionID].Key.TryGetKey(out var key) ? key.Name : missions[missionID].Key.Name;
        
        MissionService.RemoveMission(missions[missionID], save ?? false);
        
        return ChatCommandResult.Ok($"Removed {name} from rotation successfully");
    }
}