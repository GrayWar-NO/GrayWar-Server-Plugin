using BepInEx.Configuration;
using GW_server_plugin.Helpers;

namespace GW_server_plugin.Features.Voting.Sessions.Missions;

/// <summary>
///     Session for voteNext.
/// </summary>
public sealed class NextSession
    : ConfigurableVoteSession<EquatableMissionOptions>
{
    /// <inheritdoc />
    public NextSession(ConfigFile config, string? reason, AcceptableValueList<EquatableMissionOptions> missions) :
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
    public override string SessionName => "Next Mission";
    
    /// <inheritdoc />
    public override string ShortSessionName => "next";
    
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