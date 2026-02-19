using System;
using System.Collections.Generic;
using GW_server_plugin.Helpers;

namespace GW_server_plugin.Features;

/// <summary>
/// Service for managing votekicks.
/// </summary>
public class VoteKickService
{
    private readonly Dictionary<ulong, HashSet<ulong>> _votesByTarget = new();

    /// <summary>
    /// Adds a vote to the current votekicks
    /// </summary>
    /// <param name="targetId">steamId of the person to kick</param>
    /// <param name="voterId">steamId of the voter</param>
    /// <returns></returns>
    public bool AddVote(ulong targetId, ulong voterId)
    {
        if (_votesByTarget.TryGetValue(targetId, out var voters))
            voters.Add(voterId);
        else
        {
            voters = [voterId];
            _votesByTarget[targetId] = voters;
        }

        CheckVotes();
        return voters.Count != 0;
    }
    
    private void CheckVotes()
    {
        var minVotesForKick = (Globals.AuthenticatedPlayers.Count + 1) / 2;
        foreach (var kvp in _votesByTarget)
        {
            if (kvp.Value.Count > minVotesForKick && PlayerUtils.TryFindPlayerBySteamId(kvp.Key, out var player))
            {
                PlayerUtils.KickPlayer(player!, "Votekicked");
                _votesByTarget.Remove(kvp.Key);
            }
        }
    }

    /// <summary>
    /// Removes all votes that a player cast.
    /// </summary>
    /// <param name="voterId">ID of the voter to remove</param>
    public void RemoveVoter(ulong voterId)
    {
        foreach (var value in _votesByTarget.Values)
        {
            value.Remove(voterId);
        }
    }    
    
}