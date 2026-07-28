using System;
using System.Collections.Generic;
using System.Timers;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Google.Protobuf.WellKnownTypes;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

// Usually only used by VoteService. Handles generic voting sessions. the command should announce what the vote is about
// there is a 'reason' field in this class that will announce the optional reason every few ticks in the timer
// If vote passes, it will call the provided '_action'
public class VoteSession
{
    internal static VoteSession? Instance;
    
    private readonly Player _initiator;
    private readonly Timer _timer;
    private readonly HashSet<ulong> _yesVoters;
    private readonly HashSet<ulong> _noVoters;
    private int _timeLeft;
    private readonly int _voteThreshold; // don't want threshold changing as players leave or join
    private readonly string? _reason;
    private readonly string? _targetName;
    
    // If true, vote will pass ONLY IF it reaches threshold.
    // If false, vote will pass if it reaches threshold OR runs out of time and YES votes is greater than NO votes
    private readonly bool _thresholdByFullServer;

    internal bool CancelIfMissionChanges;

    // Function to call when vote succeeds
    private readonly Action _action;
    
    internal static ConfigEntry<int> KickTimeout = null!;
    internal static ConfigEntry<double> KickThreshold = null!;
    public static void Initialize(ConfigFile config)
    {
        KickTimeout = config.Bind(PluginConfig.GenericVoteServiceSection, "Kick Timeout", 180,
            "When the vote will time out in seconds");
        KickThreshold = config.Bind(PluginConfig.GenericVoteServiceSection, "Kick Threshold", 0.50,
            "Percentage in decimal format (0.0 - 1.0). YES votes ABOVE this value will pass");
    }

    public static bool CanStartVote()
    {
        return Instance == null;
    }

    /// <summary>
    /// start a vote session for target player
    /// </summary>
    /// <param name="initiator"></param>
    /// <param name="action"></param>
    /// <param name="cancelIfMissionChanges"></param>
    /// <param name="thresholdByFullServer"></param>
    /// <param name="reason"></param>
    /// <param name="targetName">The name of the target for the vote (player name, mission name)</param>
    /// <returns></returns>
    public static void StartVoteSession(Player initiator, Action action, bool cancelIfMissionChanges, bool thresholdByFullServer = true, string? reason = null, string? targetName= null)
    {
        Instance = new VoteSession(initiator, action, cancelIfMissionChanges, thresholdByFullServer, reason,
            targetName);
        Instance.Start();
    }
    
    public static void CancelVoteSession()
    {
        ChatService.SendChatMessageAsServer("WARNING: Vote session has been canceled");
        if (Instance != null)
        {
            Instance._timer.Stop();
            Instance._timer.Dispose();
            Instance = null;
        }
        GwServerPlugin.Logger.LogWarning("Invalid CancelVoteSession(), No vote session is active");
    }

    /// <summary>
    /// handles a vote from the vote command
    /// </summary>
    /// <param name="voter"></param>
    /// <param name="votedYes"></param>
    /// <param name="result"></param>
    public void HandleVote(Player voter, bool votedYes, out (bool success, string? response) result)
    {
        if (Instance == null)
            result = (false,$"A vote session has not been started, use a vote command to start one.");
        else
        {
            Instance.AddVote(voter, votedYes);
            result = (true, null);
        }
        
    }

    private VoteSession(Player initiator, Action action, bool cancelIfMissionChanges, bool thresholdByFullServer = true, string? reason = null, string? targetName= null)
    {
        _initiator = initiator;
        _voteThreshold = VoteThreshold();
        _timeLeft = KickTimeout.Value;
        _timer = new Timer(1000);
        _timer.Elapsed += OnTimerTick;
        _yesVoters = [];
        _noVoters = [];
        _action = action;
        _thresholdByFullServer = thresholdByFullServer;
        CancelIfMissionChanges = cancelIfMissionChanges;
        _reason = reason;
        _targetName = targetName;
    }

    private void _sendReminderMessage()
    {
        ChatService.SendChatMessageAsServer($"Type '{PluginConfig.CommandPrefixChar}y' for yes, '{PluginConfig.CommandPrefixChar}n' for no.");
        ChatService.SendChatMessageAsServer($"({_yesVoters.Count}/{_voteThreshold} YES votes, {_noVoters.Count}/{_voteThreshold} NO votes).");
        ChatService.SendChatMessageAsServer($"Vote expires in {_timeLeft} seconds.");
        ChatService.SendChatMessageAsServer($"Target: '{_targetName}', Reason: {_reason}");
    }

    public void Start()
    {
        _sendReminderMessage();
        _timer.Start();
        AddVote(_initiator, true);
    }

    /// <summary>
    /// Will add a vote to the vote kick if the player is not already in the hashset.
    /// </summary>
    /// <param name="voter"></param>
    /// <param name="votedYes"></param>
    public void AddVote(Player voter, bool votedYes)
    {
        if (votedYes)
        {
            if (_yesVoters.Add(voter.SteamID))
            {
                if (_yesVoters.Count >= _voteThreshold)
                {
                    ChatService.SendChatMessageAsServer("YES votes have reached a majority.");
                    FinaliseVote(true);
                }
            }
            else
            {
                ChatService.SendPrivateChatMessage("You have already voted.", voter);
            }
        }
        else
        {
            if (_noVoters.Add(voter.SteamID))
            {
                if (_noVoters.Count >= _voteThreshold)
                {
                    ChatService.SendChatMessageAsServer("NO votes have reached a majority.");
                    FinaliseVote(false);
                }
            }
            else
            {
                ChatService.SendPrivateChatMessage("You have already voted.", voter);
            }
        }
    }

    /// <summary>
    /// Callback that is called every timer tick which is set to 1 second (1000)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnTimerTick(object sender, ElapsedEventArgs e)
    {
        _timeLeft--;

        if ((_timeLeft % 30 == 0 && _timeLeft > 0) || _timeLeft < 10) // every ten seconds or below 10 seconds every tick
        {
            _sendReminderMessage();
        }
        
        if (_timeLeft <= 0)
        {
            if (_thresholdByFullServer)
                FinaliseVote(false);
            else FinaliseVote(_yesVoters.Count > _noVoters.Count);
        }
    }


    /// <summary>
    /// Checks if vote threshold is met, then calls the action function associated
    /// </summary>
    public void FinaliseVote(bool thresholdMet)
    {
        _timer.Stop();
        _timer.Dispose();
        Instance = null;
        
        if (thresholdMet)
        {
            var passMessage = $"The vote has passed! ({_yesVoters.Count}/{_voteThreshold} YES votes, {_noVoters.Count}/{_voteThreshold} NO votes)";
            var log = new ChatLog
            {
                MessageChannel = "all",
                MessageSendTime = DateTime.UtcNow.ToTimestamp(),
                Message = passMessage,
                SenderSteamID = _initiator.SteamID
            };
            GwServerPlugin.GrpcMgr.ChatLogStream?.WriteAsync(log);
            ChatService.SendChatMessageAsServer($"The vote has passed! ({_yesVoters.Count}/{_voteThreshold} YES votes, {_noVoters.Count}/{_voteThreshold} NO votes)");
            _action();
        }
        else
        {
            var failMessage = $"The vote has failed! ({_yesVoters.Count}/{_voteThreshold} YES votes, {_noVoters.Count}/{_voteThreshold} NO votes)";
            ChatService.SendChatMessageAsServer(failMessage);
        }
    }

    private int VoteThreshold()
    {
        var threshold = KickThreshold.Value;
        var totalPlayers = PlayerUtils.GetPlayerCount();
        return (int)Math.Ceiling(totalPlayers * threshold);
    }
}