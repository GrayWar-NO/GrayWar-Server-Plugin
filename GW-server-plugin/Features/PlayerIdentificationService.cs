using System.Collections.Generic;
using NuclearOption.ModdingApi;

namespace GW_server_plugin.Features;

internal static class PlayerIdentificationService
{
    private static readonly Dictionary<int, PlayerHandle> IDNameCache = new();
    
    public static void AddPlayer(PlayerHandle handle) => IDNameCache.Add(handle.PlayerIndex, handle);
    
    public static void RemovePlayer(int id) => IDNameCache.Remove(id);
    
    public static void Clear() => IDNameCache.Clear();
    
    public static bool TryGetPlayerById(int id, out PlayerHandle? player)
    {
        if (!IDNameCache.TryGetValue(id, out var playerHandle))
            player = null;
        else
            player = playerHandle;
        return player != null;
    }
}