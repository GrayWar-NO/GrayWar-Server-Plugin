using System.Globalization;
using BepInEx.Configuration;

namespace GW_server_plugin.Features.Voting.Sessions.Missions;

/// <summary>
///     VoteSession for time of day
/// </summary>
[AutoVoteSession("Time of Day", "time")]
public class TimeSession(string? reason) : ConfigurableVoteSession<TimeSession, int>(reason)
{
    /// <inheritdoc />
    protected override AcceptableValueBase AcceptableValues { get; } = new AcceptableValueRange<int>(0, 23);
    
    /// <inheritdoc />
    protected override int? DefaultVote => null;
    
    /// <inheritdoc />
    protected override string ValueStringGetter(int value) => value.ToString(CultureInfo.CurrentCulture);
    
    /// <inheritdoc />
    protected override void OnPass(int outcome) => LevelInfo.i.NetworktimeOfDay = outcome;
    
    /// <inheritdoc />
    protected override void OnFail()
    {
    }
    
    /// <inheritdoc />
    protected override bool TryParseValue(string input, out int? result)
    {
        var boolean = int.TryParse(input, out var rst);
        result = rst;
        return boolean;
    }
}