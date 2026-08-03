using System.Linq;
using BepInEx.Configuration;
using GW_server_plugin.Helpers;

namespace GW_server_plugin.Features.Voting.Sessions.Missions;

/// <summary>
///     Session for voteNext.
/// </summary>
[AutoVoteSession("Next Mission", "name")]
public sealed class NextSession
    : ConfigurableVoteSession<NextSession, EquatableMissionOptions>
{
    /// <inheritdoc />
    public NextSession(string? reason) :
        base(reason)
    {
        var acceptableValuesArray = Globals.DedicatedServerManagerInstance.missionRotation.allMissions
            .Select(av => new EquatableMissionOptions(av)).ToArray();
        AcceptableValues = new AcceptableValueList<EquatableMissionOptions>(acceptableValuesArray);
    }
    
    /// <inheritdoc />
    protected override AcceptableValueBase AcceptableValues { get; }
    
    /// <inheritdoc />
    protected override EquatableMissionOptions? DefaultVote => null;
    
    /// <inheritdoc />
    protected override string ValueStringGetter(EquatableMissionOptions value) => value.Options.Key.Name;
    
    
    /// <inheritdoc />
    protected override void OnPass(EquatableMissionOptions outcome)
    {
        Globals.DedicatedServerManagerInstance.missionRotation.OverrideNext(outcome.Options);
    }
    
    /// <inheritdoc />
    protected override void OnFail()
    {
    }
}