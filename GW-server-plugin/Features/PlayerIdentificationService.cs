using System;
using System.Collections.Generic;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features;

internal sealed class PlayerIdentificationService
{
    private readonly Dictionary<ulong, int> _players  = new();
    private readonly Stack<int> _ids =  new Stack<int>();

    public PlayerIdentificationService()
    {
        for (int i = Globals.DedicatedServerManagerInstance.Config.MaxPlayers; i > 0; i--)
        {
            _ids.Push(i);
        }
    }

    public void AssignNewPlayer(Player player)
    {
        var steamID = player.SteamID;
        if (!_players.ContainsKey(player.SteamID))
        {
            _players.Add(steamID, _ids.Pop());
        }
    }

    public void RemovePlayer(Player player)
    {
        var found = _players.TryGetValue(player.SteamID, out var id);
        if (!found) return;
        _ids.Push(id);
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