using NuclearOption.Networking;

namespace GW_server_plugin.Features.Voting.Sessions.Missions;

/// <summary>
///     Session for voteSkip.
/// </summary>
[AutoVoteSession("Mission Skip", "skip")]
public sealed class SkipSession(Player initiator, string? reason)
    : CommonMissionSession<SkipSession>(initiator, reason)
{
    /// <inheritdoc />
    protected override EquatableMissionOptions? DefaultVote { get; } =
        new(MissionService.GetNextMissionOptions(false)!.Value);
    
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