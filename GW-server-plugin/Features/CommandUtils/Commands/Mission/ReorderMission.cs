using System;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.DedicatedServer;

namespace GW_server_plugin.Features.CommandUtils.Commands.Mission;

/// <summary>
///     Command to reorder missions.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class ReorderMission(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string OutputName => "reorder-mission";
    
    /// <inheritdoc />
    public override string Description =>
        "Changes a mission's place in the rotation queue. Only has effect on \"sequence\" rotation type";
    
    /// <inheritdoc />
    public override string Usage =>
        "reorder-mission <int currentID> <int targetID> <optional bool save (default: false)\n" +
        "Moves mission at currentID to targetID, shifting everything in between up or down accordingly.";
    
    /// <inheritdoc />
    protected override string[] DefaultAliases => ["romission", "ro-mission", "mvmission"];
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
    
    /// <inheritdoc />
    public override UniTask<bool> Validate(string[] args)
    {
        var result = args.Length is >= 1 and <= 3 &&
                     int.TryParse(args[0], out _) && int.TryParse(args[1], out _)
                     && (args.Length != 3 || bool.TryParse(args[2], out _));
        return UniTask.FromResult(result);
    }
    
    /// <inheritdoc />
    public override UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var missions = MissionService.GetAllAvailableMissionOptions();
        
        var startingID = int.Parse(args[0]);
        var targetID = int.Parse(args[1]);
        
        bool? save = null;
        if (args.Length == 3)
            save = bool.Parse(args[2]);
        
        if (startingID >= missions.Length || targetID >= missions.Length)
            return UniTask.FromResult((
                false,
                $"One of {startingID} or {targetID} is not a valid missionID. Max is {missions.Length - 1}. Use {PluginConfig.CommandPrefixChar}missions to get list."
            ))!;
        
        var dsm = Globals.DedicatedServerManagerInstance;
        var oldMr = dsm.missionRotation!;
        
        var ml = new MissionOptions[oldMr.allMissions.Count];
        oldMr.allMissions.CopyTo(ml);
        
        var mission = ml[startingID];
        
        var offset = Math.Sign(targetID - startingID);
        
        for (var i = startingID; i != targetID; i += offset)
        {
            ml[i] = ml[i + offset];
        }
        
        ml[targetID] = mission;
        
        MissionService.ReloadMissionRotation(ml, save ?? false);
        return UniTask.FromResult((true, $"Mission at index {startingID} successfully moved to {targetID}!"))!;
    }
}