using System;
using BepInEx.Configuration;

namespace GW_server_plugin.Features.Voting;

/// <summary>
///     VoteSession with ways to configure common parameters
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ConfigurableVoteSession<T>(string? r) : VoteSession<T>(r)
    where T : struct, IEquatable<T>
{
    // ReSharper disable StaticMemberInGenericType
    private static ConfigEntry<int>? _minVoteValidityConfig;
    private static ConfigEntry<float>? _minAttendanceConfig;
    private static ConfigEntry<int>? _voteTimeoutSecs;
    
    private static int _configVoteTimeoutSecs;
    // ReSharper restore StaticMemberInGenericType
    
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
    public override int VoteTimeoutSeconds {get => _configVoteTimeoutSecs; set => _configVoteTimeoutSecs = value; }
}