using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
using GW_server_plugin.Features.IPC.Packets;
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

        OnMissionChanged(mission);

        return result;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="__result"></param>
    [HarmonyPatch(nameof(DedicatedServerManager.GameShouldStop))]
    [HarmonyPostfix]
    public static void GameShouldStopPatch(DedicatedServerManager __instance, ref bool __result)
    {
        if (__result) OnMissionChanged(null);
    }
    
    /// <summary>
    /// Behaviour to run whenever a mission changes.
    /// </summary>
    /// <param name="mission"></param>
    internal static void OnMissionChanged(Mission? mission)
    {
        GwServerPlugin.Logger.LogDebug($"Mission changed: {mission?.Name ?? "null"}");
        var name = mission?.Name ?? "null";
        if (ulong.TryParse(name, out var workshopID))
            if (MissionNameFix.GetMissionName(workshopID, out var workshopName))
                name = workshopName!;
        var missionChangePacket = new LogEntryPacket
        {
            Channel = LogChannel.MissionStatus,
            LogText = name
        };
        
        GwServerPlugin.LoggingOutBox.Add(missionChangePacket);
        GwServerPlugin.WarnService.ClearWarns();
    }
}   
