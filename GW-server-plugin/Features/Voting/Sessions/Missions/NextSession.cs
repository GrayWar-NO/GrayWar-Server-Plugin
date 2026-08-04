using System.Linq;
using BepInEx.Configuration;
using GW_server_plugin.Helpers;

namespace GW_server_plugin.Features.Voting.Sessions.Missions;

/// <summary>
///     Session for voteNext.
/// </summary>
[AutoVoteSession("Next Mission", "next")]
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
    
    /// <inheritdoc />
    protected override bool TryParseValue(string input, out EquatableMissionOptions? result)
    {
        result = null;
        if (AcceptableValues is AcceptableValueList<EquatableMissionOptions> avl)
        {
            var validValues = avl.AcceptableValues.Where(m => ValueStringGetter(m) == input).ToList();
            if (validValues.Any()) return false;
            result = validValues.First();
            return true;
        }
        GwServerPlugin.Logger.LogError("AcceptableValues is not the correct type in SkipSession. What the fuck??");
        return false;
    }
}