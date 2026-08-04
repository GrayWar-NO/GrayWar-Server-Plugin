using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.Voting.Sessions.Missions;

/// <summary>
///     Session for voteNext.
/// </summary>
[AutoVoteSession("Next Mission", "next")]
public sealed class NextSession(Player initiator, string? reason)
    : CommonMissionSession<NextSession>(initiator, reason)
{
    /// <inheritdoc />
    protected override EquatableMissionOptions? DefaultVote => null;
    
    
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