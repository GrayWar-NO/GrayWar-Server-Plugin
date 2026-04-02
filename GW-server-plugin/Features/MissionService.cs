using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.DedicatedServer;
using NuclearOption.Networking.Lobbies;
using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using UnityEngine;

namespace GW_server_plugin.Features;

/// <summary>
///     Manages missions on the server.
/// </summary>
public static class MissionService
{
    /// <summary>
    ///     The last mission that was started.
    /// </summary>
    public static Mission? LastMission { get; private set; }
    
    /// <summary>
    ///     The current mission.
    /// </summary>
    public static Mission? CurrentMission => Globals.DedicatedServerManagerInstance.currentMission;
    
    /// <summary>
    ///     The current mission runner.
    /// </summary>
    public static MissionRunner? CurrentMissionRunner => MissionManager.Runner;

    /// <summary>
    ///     The current mission objectives.
    /// </summary>
    public static MissionObjectives? CurrentMissionObjectives => MissionManager.Objectives;

    /// <summary>
    ///     The current mission active objectives.
    /// </summary>
    public static List<Objective> CurrentMissionActiveObjectives => CurrentMissionRunner?.ActiveObjectives ?? [];
    
    /// <summary>
    ///     The current mission time.
    /// </summary>
    public static float CurrentMissionTime => Time.timeSinceLevelLoad;
    
    /// <summary>
    ///     Gets a list of available missions
    /// </summary>
    /// <returns> The list of mission keys.</returns>
    public static MissionOptions[] GetAllAvailableMissionOptions()
    {
        MissionRotation mr = Globals.DedicatedServerManagerInstance.missionRotation;
        return mr.allMissions.ToArray();
    }
    
    /// <summary>
    ///     Get the missionOptions object for a mission from it's index in GetAllAvailableMissionKeys()
    /// </summary>
    /// <param name="index">index to look for</param>
    /// <returns></returns>
    public static MissionOptions? GetMissionOptionByIndex(int index)
    {
        MissionOptions[] missions = GetAllAvailableMissionOptions();
        if (missions.Length == 0 || (missions.Length - 1) < index) return null;
        return missions[index];
    }

    /// <summary>
    /// Get the MissionOptions object for the next mission in rotation.
    /// </summary>
    /// <returns>the missionOptions object</returns>
    public static MissionOptions? GetNextMissionOptions()
    {
        var dsm = Globals.DedicatedServerManagerInstance;
        if (dsm == null)
        {
            GwServerPlugin.Logger.LogWarning("dsm is null");
            return null;
        }

        var mr = dsm.missionRotation;
        if (mr == null)
        {
            GwServerPlugin.Logger.LogWarning("missionRotation is null");
            return null;
        }

        return mr.GetNext();

    }
    
    
    /// <summary>
    ///     Start a specific mission based on MissionOptions.
    /// </summary>
    /// <param name="missionOptions">MissionOptions for the mission to start.</param>
    public static async Task<bool> StartMission(MissionOptions missionOptions)
    {
        try
        {
            if (!missionOptions.Key.TryGetKey(out var key))
            {
                GwServerPlugin.Logger.LogWarning("Error: could not resolve mission key.");
                return false;
            }

            if (!MissionSaveLoad.TryLoad(key, out var mission, out var err))
            {
                GwServerPlugin.Logger.LogWarning($"Load failed: {err}");
                return false;
            }

            GwServerPlugin.Logger.LogInfo($"Loading next mission: {mission?.Name ?? "<unnamed>"}");

            // Switch to main thread for Unity scene/lobby ops
            await UniTask.SwitchToMainThread();
            var dsm = Globals.DedicatedServerManagerInstance;

            while (!missionOptions.Equals(dsm.missionRotation.GetNext())){}

            dsm.UpdateLobby(mission, false);
            var ok = await dsm.LoadNext(mission);
            if (!ok)
            { 
                GwServerPlugin.Logger.LogError("Failed to load next mission.");
                return false;
            }

            dsm.keyValues.SetKeyValue("start_time", LobbyInstance.CreateStartTime());
            dsm.currentMission = mission;
            dsm.currentMissionOption = missionOptions;
            LastMission = mission;
            return true;
        }
        catch (Exception e)
        {
            GwServerPlugin.Logger.LogError(e);
            GwServerPlugin.Logger.LogError("Unexpected error while loading mission.");
            return false;
        }
    }
    
    /// <summary>
    ///     Starts the next mission in the mission rotation.
    /// </summary>
    public static async Task<bool> StartNextMission()
    {
        var missionOpt = GetNextMissionOptions();
        if (missionOpt == null) return false;
        return await StartMission(missionOpt.Value);
    }
}