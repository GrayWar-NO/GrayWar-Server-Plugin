using System;
using System.Reflection;
using BepInEx.Configuration;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.Voting;

/// <summary>
///     VoteSession with ways to configure common parameters
/// </summary>
public abstract class ConfigurableVoteSession<TSession, TOutcome>(Player initiator, string? r) : VoteSession<TOutcome>(initiator, r)
    where TSession : ConfigurableVoteSession<TSession, TOutcome>
    where TOutcome : struct, IEquatable<TOutcome>
{
    // Caches the attribute once when TSession is first accessed
    private static readonly AutoVoteSessionAttribute? SessionAttribute =
        typeof(TSession).GetCustomAttribute<AutoVoteSessionAttribute>();
    
    // Properties pull directly from the attribute with fallbacks
    /// <inheritdoc />
    public override string SessionName =>
        SessionAttribute?.SessionName ?? typeof(TSession).Name;
    
    /// <inheritdoc />
    public override string ShortSessionName =>
        SessionAttribute?.ShortName ?? typeof(TSession).Name.ToLower();
    
    // ReSharper disable StaticMemberInGenericType
    private static ConfigEntry<int>? _minVoteValidityConfig;
    private static ConfigEntry<float>? _minAttendanceConfig;
    private static ConfigEntry<int>? _voteTimeoutSecs;
    
    private static int _configVoteTimeoutSecs;
    // ReSharper restore StaticMemberInGenericType
    
    /// <summary>
    ///     Initialises the config file from a 
    /// </summary>
    /// <param name="config"></param>
    /// <param name="category"></param>
    protected static void InitializeConfig(ConfigFile config, string category)
    {
        _minVoteValidityConfig ??= config.Bind(
            category,
            "Minimum vote validity",
            3,
            new ConfigDescription(
                "Minimum number of voters for a vote to be considered valid",
                new AcceptableValueRange<int>(0, 10)));
        
        _minAttendanceConfig ??= config.Bind(
            category,
            "Minimum attendance",
            0.2f,
            new ConfigDescription(
                "Minimum fraction of the current playerbase for a vote to be considered valid",
                new AcceptableValueRange<float>(0f, 1f)));
        
        _voteTimeoutSecs ??= config.Bind(
            category,
            "Vote Timeout seconds",
            120,
            "Number of seconds before the vote times out. Only evaluated in increments of 30.");
            
        _configVoteTimeoutSecs = _voteTimeoutSecs.Value;
    }
    
    /// <inheritdoc />
    public override int MinVoteValidity => _minVoteValidityConfig!.Value;
    
    /// <inheritdoc />
    public override float MinAttendance => _minAttendanceConfig!.Value;
    
    /// <inheritdoc />
    public override int VoteTimeoutSeconds 
    {
        get => _configVoteTimeoutSecs; 
        set => _configVoteTimeoutSecs = value; 
    }
}