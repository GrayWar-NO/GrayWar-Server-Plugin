using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Google.Protobuf.WellKnownTypes;
using GW_server_plugin.Events;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;
using NuclearOption.SavedMission;
using UnityEngine;
using Enum = System.Enum;

namespace GW_server_plugin.Features.Voting;

/// <summary>
///     Abstract class for defining vote sessions.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class VoteSession<T>(string? reason)
    : IVoteSession
    where T : struct, IEquatable<T>
{
    private Player _initiator = null!;
    
    // ReSharper disable once StaticMemberInGenericType
    private static readonly HashSet<string> NoValues =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "n",
            "no"
        };
    
    // ReSharper disable once StaticMemberInGenericType
    private static readonly HashSet<string> YesValues =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "y",
            "yes"
        };
    
    /// <summary>
    ///     Acceptable values for a vote.
    /// </summary>
    protected abstract AcceptableValueBase AcceptableValues { get; }
    
    private readonly Dictionary<ulong, Outcome> _votes = new();
    
    private int _timeLeft;
    
    /// <summary>
    ///     Minimum number of votes for a vote to be valid.
    /// </summary>
    public abstract int MinVoteValidity { get; }
    
    /// <summary>
    ///     Minimum fraction of player count for a vote to be valid (0-1)
    /// </summary>
    public abstract float MinAttendance { get; }
    
    /// <summary>
    ///     Max time this voteSession will go on for in seconds.
    /// </summary>
    public abstract int VoteTimeoutSeconds { get; set; }
    
    /// <summary>
    ///     Default value of the vote.
    /// </summary>
    protected abstract T DefaultVote { get; }
    
    
    private int NYesVotes => _votes.Count(v => !v.Value.No);
    private int NNoVotes => _votes.Count(v => v.Value.No);
    private int MinVotesForValidity => Mathf.Max(MinVoteValidity, (int)(PlayerUtils.GetPlayerCount() * MinAttendance));
    private static int AutoPassLimit => _getAutoPassLimit();
    
    /// <inheritdoc />
    public abstract string SessionName { get; }
    
    public abstract string ShortSessionName { get; }
    
    /// <inheritdoc />
    public string? Reason { get; } = reason;
    
    /// <inheritdoc />
    public void Destroy()
    {
        TimeEvents.Every30Seconds -= OnTimerTick;
        _votes.Clear();
    }
    
    /// <inheritdoc />
    public bool ValidateVote(Player voter, string outcome)
    {
        return NoValues.Contains(outcome) || YesValues.Contains(outcome) || TryParseValue(outcome, out _);
    }
    
    /// <inheritdoc />
    public bool TryAddVote(Player voter, string outcome, out string response)
    {
        if (NoValues.Contains(outcome))
        {
            _votes.Add(voter.SteamID, new Outcome(true, null));
            response = $"Successfully voted NO to the current {SessionName} vote.";
            if (NNoVotes > AutoPassLimit)
                Resolve();
            return true;
        }
        
        if (YesValues.Contains(outcome))
        {
            _votes.Add(voter.SteamID, new Outcome(false, DefaultVote));
            response = $"Successfully voted the default outcome {DefaultVote} to the current {SessionName} vote.";
            if (NYesVotes > AutoPassLimit)
                Resolve();
            return true;
        }
        
        if (!TryParseValue(outcome, out var value))
        {
            response =
                $"Invalid format. Expected {typeof(T).Name}. Allowed: {AcceptableValues.ToDescriptionString().TrimStart('#', ' ')}";
            return false;
        }
        
        _votes.Add(voter.SteamID, new Outcome(false, value));
        response = $"Successfully voted {value} to the current {SessionName} vote.";
        if (NYesVotes > AutoPassLimit)
            Resolve();
        return true;
    }
    
    /// <summary>
    /// Gets the string format to display for this session's outcomes.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected abstract string ValueStringGetter(T value);
    
    /// <inheritdoc />
    public IEnumerable<string> GetAllOutcomes()
    {
        List<string> result =
            [$"({string.Join("/", NoValues)})", $"({string.Join("/", NoValues)} => {ValueStringGetter(DefaultVote)})"];
        
        if (AcceptableValues is AcceptableValueList<T> listValues)
        {
            result.AddRange(listValues.AcceptableValues.Select(ValueStringGetter));
        }
        else
        {
            var type = AcceptableValues.GetType();
            
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(AcceptableValueRange<>)) return result;
            var min = type.GetProperty("MinValue")?.GetValue(AcceptableValues);
            var max = type.GetProperty("MaxValue")?.GetValue(AcceptableValues);
            
            result.Add($"{type.Name}[{min} - {max}]");
        }
        
        return result;
    }
    
    /// <inheritdoc />
    public void Start(Player initiator)
    {
        ChatService.SendChatMessageAsServer($"Starting {SessionName} vote session!");
        _sendReminderMessage();
        _timeLeft = VoteTimeoutSeconds;
        _initiator = initiator;
        TimeEvents.Every30Seconds += OnTimerTick;
    }
    
    /// <inheritdoc />
    public void Resolve()
    {
        var nTotalVotes = _votes.Count;
        
        ChatService.SendChatMessageAsServer(
            $"Final votes: Yes: {NYesVotes}/{AutoPassLimit}\tNo: {NNoVotes}/{AutoPassLimit}");
        if (nTotalVotes >= MinVotesForValidity && (NYesVotes >= AutoPassLimit || NYesVotes > NNoVotes))
        {
            var outcome = GetWinningOutcome();
            ChatService.SendChatMessageAsServer($"{SessionName} vote passed with outcome: {outcome}!");
            OnPass(outcome);
        }
        else
        {
            ChatService.SendChatMessageAsServer($"{SessionName} vote failed!");
            OnFail();
        }
    }
    
    private static int _getAutoPassLimit()
    {
        var connectedPlayers = PlayerUtils.GetPlayerCount();
        var autoPassLimit = Mathf.CeilToInt(connectedPlayers / 2f);
        if (connectedPlayers % 2 == 0) autoPassLimit++;
        return autoPassLimit;
    }
    
    private void _sendReminderMessage()
    {
        ChatService.SendChatMessageAsServer(
            $"Type '{PluginConfig.CommandPrefixChar}y' for yes, '{PluginConfig.CommandPrefixChar}n' for no.");
        ChatService.SendChatMessageAsServer(
            $"({NYesVotes}/{AutoPassLimit} YES votes, {NNoVotes}/{AutoPassLimit} NO votes).");
        ChatService.SendChatMessageAsServer($"Vote expires in {_timeLeft} seconds. {GetWinningOutcome()} is winning.");
        ChatService.SendChatMessageAsServer(Reason == null
            ? $"Target: '{SessionName}, initiated by {_initiator.GetDisplayName()}'"
            : $"Target: '{SessionName}, Reason: '{Reason}'");
    }
    
    private void OnTimerTick()
    {
        _timeLeft -= 30;
        if (_timeLeft <= 0) Resolve();
        else _sendReminderMessage();
    }
    
    private T GetWinningOutcome()
    {
        Dictionary<T, int> values = [];
        foreach (var vote in _votes)
        {
            if (vote.Value.Value is not { } outcome) continue;
            values.TryGetValue(outcome, out var value);
            values[outcome] = value + 1;
        }
        
        var val = values.Aggregate((a, b) => a.Value > b.Value ? a : b);
        return val.Key;
    }
    
    /// <summary>
    ///     Executes when a vote passes.
    /// </summary>
    /// <param name="outcome"></param>
    protected abstract void OnPass(T outcome);
    
    /// <summary>
    ///     Executes when a vote fails.
    /// </summary>
    protected abstract void OnFail();
    
    
    private static bool TryParseValue(string input, out T result)
    {
        result = default!;
        try
        {
            if (typeof(T).IsEnum)
            {
                result = (T)Enum.Parse(typeof(T), input, true);
                return true;
            }
            
            result = (T)Convert.ChangeType(input, typeof(T));
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    
    private sealed class Outcome(bool no, T? value)
    {
        internal readonly bool No = no;
        internal readonly T? Value = value;
    }
}