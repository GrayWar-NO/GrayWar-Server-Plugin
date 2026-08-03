using System.Linq;
using BepInEx.Configuration;
using GW_server_plugin.Helpers;

namespace GW_server_plugin.Features.Voting.Sessions.Missions;

/// <summary>
///     Session for voteSkip.
/// </summary>
[AutoVoteSession("Mission Skip", "skip")]
public sealed class SkipSession
    : ConfigurableVoteSession<SkipSession, EquatableMissionOptions>
{
    /// <inheritdoc />
    public SkipSession(ConfigFile config, string? reason) :
        base(reason)
    {
        InitializeConfig(config, $"{SessionName} vote session");
        
        var acceptableValuesArray = Globals.DedicatedServerManagerInstance.missionRotation.allMissions
            .Select(av => new EquatableMissionOptions(av)).ToArray();
        AcceptableValues = new AcceptableValueList<EquatableMissionOptions>(acceptableValuesArray);
    }
    
    /// <inheritdoc />
    protected override AcceptableValueBase AcceptableValues { get; }
    
    /// <inheritdoc />
    protected override EquatableMissionOptions? DefaultVote { get; } =
        new(MissionService.GetNextMissionOptions(false)!.Value);
    
    /// <inheritdoc />
    protected override string ValueStringGetter(EquatableMissionOptions value) => value.Options.Key.Name;
    
    
    /// <inheritdoc />
    protected override void OnPass(EquatableMissionOptions outcome)
    {
        _ = MissionService.StartMission(outcome.Options);
    }
    
    /// <inheritdoc />
    protected override void OnFail()
    {
    }
}