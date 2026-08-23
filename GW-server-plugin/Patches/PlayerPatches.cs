using System;
using Com.Graywar.NoServerManager.Proto;
using Google.Protobuf.WellKnownTypes;
using GW_server_plugin.Events;
using HarmonyLib;
using NuclearOption.Networking;

namespace GW_server_plugin.Patches;

/// <summary>
///     Logs sortie status for players.
/// </summary>
[HarmonyPatch(typeof(Player))]
[HarmonyWrapSafe]
public class PlayerPatches
{
    /// <summary>
    ///     Patch for when a player gets into a plane.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="airframe"></param>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Player.FlyOwnedAirframe))]
    public static void AttachPatch(Player __instance, AircraftDefinition airframe)
    {
        var log = new SortieStatus
        {
            Start = true,
            SteamID = __instance.SteamID,
            PlaneName = airframe.unitName,
            Time = DateTime.UtcNow.ToTimestamp()
        };
        GwServerPlugin.GrpcMgr.Client?.SendSortieChangeAsync(log);
    }
    
    /// <summary>
    ///     Patch for when a player gets gracefully out of a plane.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="airframe"></param>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Player.RecoverAirframeInUse))]
    public static void RecoverPatch(Player __instance, AircraftDefinition airframe)
    {
        var log = new SortieStatus
        {
            Start = false,
            SteamID = __instance.SteamID,
            Killed = false,
            Time = DateTime.UtcNow.ToTimestamp()
        };
        GwServerPlugin.GrpcMgr.Client?.SendSortieChangeAsync(log);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Player.OnStartServer))]
    private static void JoinMessagePostfix(Player __instance)
    {
        PlayerEvents.OnPlayerJoined(__instance);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Player.OnStopServer))]
    private static void DisconnectedMessagePostfix(Player __instance)
    {
        PlayerEvents.OnPlayerLeft(__instance);
    }
}