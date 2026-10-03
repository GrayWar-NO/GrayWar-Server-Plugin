using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;

namespace GW_server_plugin.Features.CommandUtils.Commands.Mission;

/// <summary>
///     Command to remove a mission from rotation.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class RemoveMission(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string OutputName => "rmmission";
    
    /// <inheritdoc />
    public override string Description => "Removes a mission from rotation";
    
    /// <inheritdoc />
    public override string Usage => "rmmission <int missionID> <optional bool save (default: false)>";
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
    
    /// <inheritdoc />
    public override UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(
            args.Length is >= 1 and <= 2 &&
            int.TryParse(args[0], out _) &&
            (args.Length != 2 || bool.TryParse(args[1], out _))
        );
    }
    
    /// <inheritdoc />
    public override UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var missionID = int.Parse(args[0]);
        bool? save = null;
        if (args.Length == 2)
            save = bool.Parse(args[1]);
        
        var missions = MissionService.GetAllAvailableMissionOptions();
        
        if (missionID >= missions.Length)
            return UniTask.FromResult((false,
                $"{missionID} is not a valid missionID. Max is {missions.Length - 1}. Use {PluginConfig.CommandPrefixChar}missions to get list."))!;
        
        var name = missions[missionID].Key.TryGetKey(out var key) ? key.Name : missions[missionID].Key.Name;
        
        MissionService.RemoveMission(missions[missionID], save ?? false);

        return UniTask.FromResult((true, $"Removed {name} from rotation successfully"))!;
        
    }
}