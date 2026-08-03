using BepInEx.Configuration;

namespace GW_server_plugin.Features.Voting.Sessions.Missions;

/// <summary>
///     Session for voteSkip.
/// </summary>
public sealed class SkipSession
    : ConfigurableVoteSession<EquatableMissionOptions>
{
    /// <inheritdoc />
    public SkipSession(ConfigFile config, string? reason, AcceptableValueList<EquatableMissionOptions> missions) :
        base(reason)
    {
        InitializeConfig(config, $"{SessionName} vote session");
        AcceptableValues = missions;
    }
    
    /// <inheritdoc />
    protected override AcceptableValueBase AcceptableValues { get; }
    
    /// <inheritdoc />
    protected override EquatableMissionOptions DefaultVote { get; } =
        new(MissionService.GetNextMissionOptions(false)!.Value);
    
    /// <inheritdoc />
    public override string SessionName => "Mission Skip";
    
    /// <inheritdoc />
    public override string ShortSessionName => "skip";
    
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