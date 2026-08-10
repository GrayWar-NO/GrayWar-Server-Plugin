using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using GW_server_plugin.Events;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;
using UnityEngine;

// ReSharper disable UseCollectionExpression

namespace GW_server_plugin.Features.Voting;

/// <summary>
///     Abstract class for defining vote sessions.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class VoteSession<T>(Player initiator, string? reason)
    : IVoteSession
    where T : struct, IEquatable<T>
{
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

    private readonly Dictionary<ulong, Outcome> _votes = new();

    private int _timeLeft;

    /// <summary>
    ///     Acceptable values for a vote.
    /// </summary>
    protected abstract AcceptableValueBase AcceptableValues { get; }

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
    protected abstract T? DefaultVote { get; }


    private int NYesVotes => _votes.Count(v => !v.Value.No);
    private int NNoVotes => _votes.Count(v => v.Value.No);
    private int MinVotesForValidity => Mathf.Max(MinVoteValidity, (int)(PlayerUtils.GetPlayerCount() * MinAttendance));
    private static int AutoPassLimit => _getAutoPassLimit();

    /// <inheritdoc />
    public abstract string SessionName { get; }

    /// <inheritdoc />
    public abstract string ShortSessionName { get; }

    /// <inheritdoc />
    public string? Reason { get; } = reason;

    /// <inheritdoc />
    public void Destroy()
    {
        TimeEvents.Every30Seconds -= OnTimerTick;
        _votes.Clear();
        if (VoteManager.Session == this)
            VoteManager.Session = null;
    }

    /// <inheritdoc />
    public void RemoveVoter(Player voter)
    {
        _votes.Remove(voter.SteamID);
    }

    /// <inheritdoc />
    public bool ValidateVote(Player voter, string outcome) => NoValues.Contains(outcome) ||
                                                              YesValues.Contains(outcome) ||
                                                              TryParseValue(outcome, out _);

    /// <inheritdoc />
    public bool TryAddVote(Player voter, string outcome, out string response)
    {
        if (NoValues.Contains(outcome))
        {
            _votes.Add(voter.SteamID, new Outcome(true, null));
            response = $"Successfully voted NO to the current {SessionName} vote.";
            if (NNoVotes >= AutoPassLimit)
                Resolve();
            return true;
        }

        if (YesValues.Contains(outcome))
        {
            if (DefaultVote == null)
            {
                response =
                    $"cannot vote YES to {SessionName} vote: no default value is set.\nUse {PluginConfig.CommandPrefixChar}vote ? to get available options.";
                return false;
            }

            _votes.Add(voter.SteamID, new Outcome(false, DefaultVote));
            response =
                $"Successfully voted the default outcome {ValueStringGetter(DefaultVote!.Value)} to the current {SessionName} vote.";
            if (NYesVotes >= AutoPassLimit)
                Resolve();
            return true;
        }

        if (!(TryParseValue(outcome, out var value) && AcceptableValues.IsValid(value)))
        {
            response =
                $"Invalid format. Expected {typeof(T).Name}. Allowed: {AcceptableValues.ToDescriptionString().TrimStart('#', ' ')}";
            return false;
        }

        _votes.Add(voter.SteamID, new Outcome(false, value));
        response = $"Successfully voted {ValueStringGetter(value!.Value)} to the current {SessionName} vote.";
        if (NYesVotes >= AutoPassLimit)
            Resolve();
        return true;
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAllOutcomes()
    {
        List<string> result = [$"({string.Join("/", NoValues)})"];
        if (DefaultVote != null)
            result.Add($"({string.Join("/", YesValues)}) => {ValueStringGetter(DefaultVote.Value)}");

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

            result.Add($"{typeof(T).Name}[{min} - {max}]");
        }

        return result;
    }

    /// <inheritdoc />
    public void Start()
    {
        ChatService.SendChatMessageAsServer($"Starting {SessionName} vote session!");
        _timeLeft = VoteTimeoutSeconds;
        _sendReminderMessage();
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
            ChatService.SendChatMessageAsServer(
                $"{SessionName} vote passed with outcome: {ValueStringGetter(outcome)}!");
            OnPass(outcome);
        }
        else
        {
            ChatService.SendChatMessageAsServer($"{SessionName} vote failed!");
            OnFail();
        }

        Destroy();
    }

    /// <summary>
    ///     Gets the string format to display for this session's outcomes.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected abstract string ValueStringGetter(T value);

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
            $"Type '{PluginConfig.CommandPrefixChar}vote <val>' to vote. (Use '{PluginConfig.CommandPrefixChar}vote ?' to get acceptable values)");
        ChatService.SendChatMessageAsServer(
            $"({NYesVotes}/{AutoPassLimit} PASSING votes, {NNoVotes}/{AutoPassLimit} NO votes).");
        ChatService.SendChatMessageAsServer(_votes.Any()
            ? $"Vote expires in {_timeLeft} seconds. {ValueStringGetter(GetWinningOutcome())} is winning."
            : $"Vote expires in {_timeLeft} seconds.");
        ChatService.SendChatMessageAsServer($"Target: '{SessionName}', initiated by {initiator.GetDisplayName()}");
        if (Reason == null) return;
        ChatService.SendChatMessageAsServer($"Reason: {Reason}");
    }

    private void OnTimerTick()
    {
        _timeLeft -= 30;
        if (_timeLeft <= 0) Resolve();
        else _sendReminderMessage();
    }

    private T GetWinningOutcome()
    {
        if (!_votes.Any())
            throw new InvalidOperationException("Cannot get winning outcome with empty votes dict.");
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


    /// <summary>
    ///     Tries to parse a string to a T value.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    protected abstract bool TryParseValue(string input, out T? result);

    private sealed class Outcome(bool no, T? value)
    {
        internal readonly bool No = no;
        internal readonly T? Value = value;
    }
}