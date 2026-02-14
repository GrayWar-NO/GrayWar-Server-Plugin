using System;
using System.Collections.Generic;
using NuclearOption.Networking;

namespace GW_server_plugin.Features;

internal sealed class PlayerIdentificationService
{
    private readonly Dictionary<ulong, int> _players  = new();
    private Stack<int> ids =  new Stack<int>();

    public PlayerIdentificationService()
    {
        for (int i = 200; i > 0; i--)
        {
            ids.Push(i);
        }
    }

    public void AssignNewPlayer(Player player)
    {
        var steamID = player.SteamID;
        if (!_players.ContainsKey(player.SteamID))
        {
            _players.Add(steamID, ids.Pop());
        }
    }

    public void RemovePlayer(Player player)
    {
        var found = _players.TryGetValue(player.SteamID, out var id);
        if (!found) return;
        ids.Push(id);
        _players.Remove(player.SteamID);
    }

    public int GetPlayerId(Player player)
    {
        return _players[player.SteamID];
    }

    public void GetPlayerById(int id, out ulong? player)
    {
        foreach (var keyValuePair in _players)
        {
            if (keyValuePair.Value == id)
            {
                player = keyValuePair.Key;
                return;
            }
        }

        player = null;
    }
}