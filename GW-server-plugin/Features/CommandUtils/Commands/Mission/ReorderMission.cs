using System;
using BepInEx.Configuration;
using GW_server_plugin.Helpers;
using NuclearOption.DedicatedServer;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Mission;

/// <summary>
///     Command to reorder missions.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class ReorderMission(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string Name => "reorder-mission";
    
    /// <inheritdoc />
    public override string Description =>
        "Changes a mission's place in the rotation queue. Only has effect on \"sequence\" rotation type";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("currentID", ChatArgumentType.Text),// TODO waiting for INT type
        new("targetID", ChatArgumentType.Text),
        new("save", ChatArgumentType.Text, true)
    ];
    
    /// <inheritdoc />
    protected override string[] DefaultAliases => ["romission", "ro-mission", "mvmission"];
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
    
    /// <inheritdoc />
    public override ChatCommandResult Execute(string[] args)
    {
        var missions = MissionService.GetAllAvailableMissionOptions();
        
        var startingID = int.Parse(args[0]);
        var targetID = int.Parse(args[1]);
        
        bool? save = null;
        if (args.Length == 3)
            save = bool.Parse(args[2]);
        
        if (startingID >= missions.Length || targetID >= missions.Length)
            return ChatCommandResult.ClientError(
                $"One of {startingID} or {targetID} is not a valid missionID. Max is {missions.Length - 1}. Use {PluginConfig.CommandPrefixChar}missions to get list.");
        
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
        return ChatCommandResult.Ok($"Mission at index {startingID} successfully moved to {targetID}!");
    }
}