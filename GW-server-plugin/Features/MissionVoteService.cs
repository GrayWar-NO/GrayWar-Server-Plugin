using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Configuration;
using GW_server_plugin.Helpers;
using NuclearOption.DedicatedServer;

namespace GW_server_plugin.Features;

internal sealed class MissionVoteService(ConfigFile config)
{
    
    private CancellationTokenSource _voteCancelled = null!;
    
    private const string RtvCommandSection = "RTV";

    private const int DefaultVoteLength = 300;
    private ConfigEntry<int> VoteLength { get; } =
        config.Bind(RtvCommandSection, "Vote length (seconds)", DefaultVoteLength);


    private const int DefaultReminderInterval = 60;
    private ConfigEntry<int> ReminderInterval { get; } =
        config.Bind(RtvCommandSection, "Reminder interval (seconds)", DefaultReminderInterval);

    private const int DefaultMapSwitchDelay = 30;
    private ConfigEntry<int> MapSwitchDelay { get; } =
        config.Bind(RtvCommandSection, "Map Switch delay (seconds)", DefaultMapSwitchDelay);

    private const int DefaultMinVoteValidity = 3;
    private ConfigEntry<int> MinVoteValidity { get; } = config.Bind(RtvCommandSection, "Minimum number of players for a vote to be valid", DefaultMinVoteValidity);

    private readonly HashSet<ulong> _yesVotes = [];
    private readonly HashSet<ulong> _noVotes = [];

    private readonly Dictionary<int, List<ulong>> _missionVotes = new();

    private MissionOptions[] Missions { get; set; } = null!;
    private int DefaultMissionIndex { get; set; }

    private MissionOptions MapVoteWinner { get; set; }

    private bool _rtvActive;

    private bool _rtvInhibited;

    private string? _rtvInhibitReason;


    public void Inhibit(string reason)
    {
        ResetRtv();
        _rtvInhibited = true;
        _rtvInhibitReason = reason;
    }

    public void ClearInhibit()
    {
        _rtvInhibited = false;
    }

    public bool RegisterRtv(ulong steamid, bool yes, int? missionIndex, out string? result)
    {
        if (_rtvInhibited)
        {
            result = $"RTV is inhibited: {_rtvInhibitReason}";
            return false;
        }

        if (!_rtvActive) StartRtv();
        missionIndex ??= DefaultMissionIndex;
        RemoveVoter(steamid);
        if (yes)
        {
            RegisterMissionVote(steamid, missionIndex.Value);
            _yesVotes.Add(steamid);
        }
        else
        {
            _noVotes.Add(steamid);
        }

        DisplayRtvCount();
        result = null;
        return true;
    }

    private void StartRtv()
    {
        Missions = MissionService.GetAllAvailableMissionOptions();
        DefaultMissionIndex = Array.IndexOf(Missions, MissionService.GetNextMissionOptions() ?? Missions[0]);
        if (DefaultMissionIndex == -1) DefaultMissionIndex = 0;
        ResetRtv();
        ChatService.SendChatMessageAsServer("-- Rock-The-Vote --");
        ChatService.SendChatMessageAsServer("Mission voting has started!");
        ChatService.SendChatMessageAsServer("Use /rtv vote to change the mission.");
        ChatService.SendChatMessageAsServer("Use /help rtv for more info.");
        _rtvActive = true;
        _voteCancelled = new CancellationTokenSource();
        VoteOngoing();
    }

    private void ResetRtv()
    {
        _yesVotes.Clear();
        _noVotes.Clear();
        _missionVotes.Clear();
        _rtvActive = false;
        _voteCancelled.Cancel();
    }


    public void RemoveVoter(ulong steamid)
    {
        if (_noVotes.Remove(steamid)) return;
        if (!_yesVotes.Remove(steamid)) return;
        foreach (var mission in _missionVotes.Keys)
        {
            _missionVotes.TryGetValue(mission, out var missionVotes);
            if (missionVotes == null) continue;
            if (missionVotes.Contains(steamid)) missionVotes.Remove(steamid);
        }
    }


    private void RegisterMissionVote(ulong steamid, int missionIndex)
    {
        if (!_rtvActive) return;
        if (missionIndex < 0 || missionIndex >= Missions.Length) return;
        _missionVotes.TryGetValue(missionIndex, out var missionVotes);
        if (missionVotes == null)
        {
            var newMissionVotes = new List<ulong> { steamid };
            _missionVotes[missionIndex] = newMissionVotes;
            return;
        }
        missionVotes.Add(steamid);
    }
    
    private void DisplayRtvCount()
    {
        var connectedPlayers = Globals.AuthenticatedPlayers.Count - 1;

        var nYesVotes = _yesVotes.Count;
        var nNoVotes = _noVotes.Count;
        var autoPassLimit = Math.Ceiling((double)connectedPlayers / 2f);
        if (connectedPlayers % 2 == 0) autoPassLimit++; // make it absolute majority.
        ChatService.SendChatMessageAsServer($"RTV: Yes: {nYesVotes}/{autoPassLimit}\tNo: {nNoVotes}/{autoPassLimit}");
        if (nYesVotes >= autoPassLimit) { PassVote(); }
        else if (nNoVotes >= autoPassLimit) { FailVote(); }
    }

    private MissionOptions GetVoteWinner()
    {
        var winner = _missionVotes
            .OrderByDescending(kvp => kvp.Value.Count)
            .First();

        return MissionService.GetMissionOptionByIndex(winner.Key) ?? MissionService.GetNextMissionOptions()!.Value;
    }


    private async void WaitBeforeMapSwitch()
    {
        try
        {
            Inhibit("Mission is awaiting switch");
            await Task.Delay(MapSwitchDelay.Value * 1000);
            ClearInhibit();
            ChatService.SendChatMessageAsServer($"Switching map now!");
            _ = MissionService.StartMission(MapVoteWinner);
        }
        catch (Exception e)
        {
            GwServerPlugin.Logger.LogError(e.ToString());
        }
    }

    private void EndVote()
    {
        ChatService.SendChatMessageAsServer($"RTV is ending");
        var connectedPlayers = Globals.AuthenticatedPlayers.Count - 1;
        var nYesVotes = _yesVotes.Count;
        var nNoVotes = _noVotes.Count;
        var autoPassLimit = Math.Ceiling((double)connectedPlayers / 2f);
        if (connectedPlayers % 2 == 0) autoPassLimit++; // make it absolute majority.
        ChatService.SendChatMessageAsServer($"Final votes: Yes: {nYesVotes}/{autoPassLimit}\tNo: {nNoVotes}/{autoPassLimit}");
        if (nYesVotes >= autoPassLimit || (nYesVotes >= MinVoteValidity.Value && nYesVotes > nNoVotes))
        {
            PassVote();
        }
        else
        {
            FailVote();
        }
    }
    
    private void PassVote()
    {
        MapVoteWinner = GetVoteWinner();
        ChatService.SendChatMessageAsServer("RTV has succeeded!");
        ChatService.SendChatMessageAsServer($"Mission will switch to {MapVoteWinner.Key.Name} in {MapSwitchDelay.Value} seconds.");
        WaitBeforeMapSwitch();
        GwServerPlugin.Logger.LogInfo("RTV Success!");
        ResetRtv();
    }

    private void FailVote()
    {
        ResetRtv();
        ChatService.SendChatMessageAsServer("RTV has failed!");
        ChatService.SendChatMessageAsServer("Mission will not switch.");
        GwServerPlugin.Logger.LogInfo("RTV Failed!");
    }

    private async void VoteOngoing()
    {
        try
        {
            var ct = _voteCancelled.Token;
            var timeRemaining = VoteLength.Value;
            while (timeRemaining > ReminderInterval.Value)
            {
                await Task.Delay(ReminderInterval.Value * 1000, ct);
                timeRemaining -= ReminderInterval.Value;
                ChatService.SendChatMessageAsServer($"RTV is ongoing. {timeRemaining} seconds remaining.");
                DisplayRtvCount();
            }
            await Task.Delay(timeRemaining * 1000, ct);
            EndVote();
        } 
        catch (OperationCanceledException) {}
        catch  (Exception e)
        {
            GwServerPlugin.Logger.LogError(e);
        }
    }
}