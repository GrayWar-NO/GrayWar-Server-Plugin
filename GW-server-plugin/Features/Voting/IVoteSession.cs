using System.Collections.Generic;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.Voting;

/// <summary>
///     Interface class for defining vote sessions.
/// </summary>
public interface IVoteSession
{
    /// <summary>
    ///     Name of the current vote session.
    /// </summary>
    string SessionName { get; }
    
    /// <summary>
    ///     Short name for the current vote session.
    /// </summary>
    string ShortSessionName { get; }
    
    /// <summary>
    ///     Reason the vote session was started for.
    /// </summary>
    string? Reason { get; }
    
    /// <summary>
    ///     Starts the voteSession behaviours, like vote reminders and such.
    /// </summary>
    void Start(Player initiator);
    
    /// <summary>
    ///     Gets executed when vote ends. Resolves outcomes and executes desired behaviour.
    /// </summary>
    void Resolve();
    
    /// <summary>
    ///     Destroy the session, remove all votes and stop the timer.
    /// </summary>
    void Destroy();
    
    /// <summary>
    ///     Tries to add a vote to the session. 
    /// </summary>
    /// <param name="voter">user that is voting</param>
    /// <param name="outcome">Outcome string the user is voting for</param>
    /// <param name="response">Descriptive response string</param>
    /// <returns>true if adding a vote was successful</returns>
    bool TryAddVote(Player voter, string outcome, out string response);
    
    /// <summary>
    ///     Removes votes from a player.
    /// </summary>
    /// <param name="voter"></param>
    void RemoveVoter(Player voter);
    
    /// <summary>
    ///     Returns true if the given outcome is a valid vote, false otherwise.
    /// </summary>
    /// <param name="voter"></param>
    /// <param name="outcome"></param>
    /// <returns></returns>
    bool ValidateVote(Player voter, string outcome);
    
    /// <summary>
    /// Gets all outcome strings for this session.
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> GetAllOutcomes();
}