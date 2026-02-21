using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BepInEx.Configuration;
using GW_server_plugin.Helpers;
using NuclearOption.DedicatedServer;

namespace GW_server_plugin.Features;

internal sealed class MissionVoteService(ConfigFile config)
{
    private const string RtvCommandSection = "RTV";

    private const int DefaultVoteLength = 300;
    private ConfigEntry<int> VoteLength { get; } = config.Bind(RtvCommandSection, "Vote length (seconds)", DefaultVoteLength);

    private const int DefaultMapSwitchDelay = 30;
    private ConfigEntry<int> MapSwitchDelay { get; } = config.Bind(RtvCommandSection, "Map Switch delay (seconds)", DefaultMapSwitchDelay);

    private readonly HashSet<ulong> RTVs = [];
    private readonly Dictionary<MissionOptions, List<ulong>> _missionVotes = new Dictionary<MissionOptions, List<ulong>>();
    private MissionOptions[] Missions { get; set; } = null!;
    private MissionOptions? DefaultMissionVote { get; set; }

    private MissionOptions MapVoteWinner { get; set; }

    public bool RtvActive;

    public bool RegisterRtv(ulong steamid, int? missionIndex)
    {
        if (!RtvActive) StartRtv();
        MissionOptions? mission;
        if (missionIndex == null) mission = DefaultMissionVote!.Value;
        else mission = MissionService.GetMissionOptionByIndex(missionIndex.Value);
        if (mission == null) return false;
        RTVs.Add(steamid);
        RegisterMissionVote(steamid, mission.Value);
        CheckRtvCount();
        return true;
    }

    private void StartRtv()
    {
        Missions = MissionService.GetAllAvailableMissionOptions();
        DefaultMissionVote = MissionService.GetNextMissionOptions();
        DefaultMissionVote ??= Missions[0]; // Here in case GetNextMissionOption fails.
        _missionVotes.Clear();
        RTVs.Clear();
        ChatService.SendChatMessage("-- Rock-The-Vote --");
        ChatService.SendChatMessage("Mission voting has started!");
        ChatService.SendChatMessage("Use /rtv vote for changing the mission.");
        ChatService.SendChatMessage("Use /help rtv for more info.");
        RtvActive = true;
        WaitForVoteEnd();
    }

    public void ResetRtv()
    {
        RTVs.Clear();
        _missionVotes.Clear();
        RtvActive = false;
    }
    

    public void RemoveExistingMissionVote(ulong steamid)
    {
        foreach (var mission in _missionVotes.Keys)
        {
            _missionVotes.TryGetValue(mission, out var missionVotes);
            if (missionVotes == null) continue;
            if (missionVotes.Contains(steamid)) missionVotes.Remove(steamid);
        }
    }

    
    private bool RegisterMissionVote(ulong steamid, MissionOptions mission)
    {
        if (!RtvActive) return false;
        if (!Missions.Contains(mission)) return false;
        RemoveExistingMissionVote(steamid);
        _missionVotes.TryGetValue(mission, out var missionVotes);
        if (missionVotes == null)
        {
            var newMissionVotes = new List<ulong> { steamid };
            _missionVotes[mission] = newMissionVotes;
            return true;
        }
        missionVotes.Add(steamid);
        return true;
    }

    public void RemoveVoter(ulong steamid)
    {
        RemoveExistingMissionVote(steamid);
        RTVs.Remove(steamid);
    }

    private void CheckRtvCount()
    {
        var connectedPlayers = Globals.AuthenticatedPlayers.Count - 1;
        var minLimit = (connectedPlayers + 1) / 2;
        if (connectedPlayers <= 1) return;
        ChatService.SendChatMessage($"RTV: {RTVs.Count} / {minLimit} ");

        if (RTVs.Count >= minLimit)
        {
            ExecuteRtv();
            GwServerPlugin.Logger.LogInfo("RTV Success!");
        }
    }

    private MissionOptions GetVoteWinner()
    {
        var winnerKvp = _missionVotes.First();
        var winner = winnerKvp.Key;
        var winnerCount = winnerKvp.Value.Count;

        foreach (var kvp in _missionVotes)
        {
            if (kvp.Value.Count > winnerCount)
            {
                winnerCount = kvp.Value.Count;
                winner = kvp.Key;
            }
        }
        return winner;
    }

    private void ExecuteRtv()
    {
        MapVoteWinner = GetVoteWinner();
        ChatService.SendChatMessage("RTV has succeeded!");
        ChatService.SendChatMessage($"Mission will switch to {MapVoteWinner.Key.Name} in {MapSwitchDelay.Value} seconds.");
        WaitBeforeMapSwitch();
        ResetRtv();
    }
    
    private async void WaitBeforeMapSwitch()
    {
        try
        {
            await Task.Delay(MapSwitchDelay.Value * 1000);
            ChatService.SendChatMessage($"Switching map now!");
            _ = MissionService.StartMission(MapVoteWinner);
        }
        catch (Exception e)
        {
            GwServerPlugin.Logger.LogError(e.ToString());
        }
    }

    private void EndVote()
    {
        ResetRtv();
        ChatService.SendChatMessage("Mission vote has failed!");
        ChatService.SendChatMessage("Mission will not switch.");
    }

    private async void WaitForVoteEnd()
    {
        try
        {
            await Task.Delay(VoteLength.Value * 1000);
            if (RtvActive)
            {
                EndVote();
            }
        }
        catch (Exception e)
        {
            GwServerPlugin.Logger.LogError(e.ToString());
        }
        
    }
}
