using NuclearOption.Networking;

namespace GW_server_plugin.Features.Voting;

/// <summary>
///     Manages vote sessions, starting vote sessions and the like.
/// </summary>
public static class VoteManager
{
    /// <summary>
    ///     Currently in use votesession or null
    /// </summary>
    private static IVoteSession? _session;
    
    /// <summary>
    ///     Starts a voteSession.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="initiator"></param>
    /// <param name="outcome"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    public static bool TryStartVote(IVoteSession session, Player initiator, string outcome, out string? response)
    {
        response = null;
        if (_session != null) return false;
        _session = session;
        _session.Start(initiator);
        return _session.TryAddVote(initiator, outcome, out response);
    }
    
    /// <summary>
    ///     Destroys the current voteSession.
    /// </summary>
    public static void CancelVote()
    {
        _session?.Destroy();
        _session = null;
    }
}