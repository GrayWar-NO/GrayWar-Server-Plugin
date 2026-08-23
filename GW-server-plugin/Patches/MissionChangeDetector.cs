using System;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using HarmonyLib;
using NuclearOption.DedicatedServer;
using NuclearOption.SavedMission;

namespace GW_server_plugin.Patches;

/// <summary>
/// Detects changes to the mission state
/// </summary>
[HarmonyPatch(typeof(DedicatedServerManager))]
public class MissionChangeDetector
{
    [HarmonyPatch(nameof(DedicatedServerManager.LoadMissionMap))]
    static void Postfix(DedicatedServerManager __instance, Mission mission, ref UniTask<bool> __result)
    {
        __result = AwaitResult(mission, __result);
    }

    static async UniTask<bool> AwaitResult(Mission mission, UniTask<bool> originalTask)
    {
        bool result = await originalTask;
        if (!result) return false;

        try
        {
            OnMissionStart(mission);
        }
        catch (Exception exception)
        {
            // Mission reporting is optional and must never make the game server
            // fail a successfully loaded mission.
            GwServerPlugin.Logger.LogError($"Failed to report mission change: {exception}");
        }

        return result;
    }
    
    /// <summary>
    /// Behaviour to run whenever a mission starts.
    /// </summary>
    /// <param name="mission"></param>
    internal static void OnMissionStart(Mission mission)
    {
        GwServerPlugin.Logger.LogDebug($"Mission changed: {mission.Name}");
        var log = new MissionStatus
        {
            MissionName = mission.Name,
            Ended = false,
            Time = DateTime.UtcNow.ToTimestamp()
        };
        GwServerPlugin.GrpcMgr.Client?.SendMissionChangeAsync(log);
        
        GwServerPlugin.WarnService.ClearWarns();
    }
}   
