using GW_server_plugin.Events;
using HarmonyLib;
using NuclearOption.Networking;

namespace GW_server_plugin.Patches;

/// <summary>
/// Detects changes in faction for a player.
/// </summary>
[HarmonyPatch(typeof(FactionHQ))]
[HarmonyWrapSafe]
public class FactionChangeDetector
{
    /// <summary>
    /// Prefixes FactionHQ.AddPlayer to get the faction change.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="player"></param>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(FactionHQ.AddPlayer))]
    public static void AddPlayerPrefix(FactionHQ __instance, Player player)
    {
        PlayerEvents.OnPlayerJoinFaction(player, __instance);
    }
}