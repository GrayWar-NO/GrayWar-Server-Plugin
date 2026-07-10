using Com.Graywar.NoServerManager.Proto;
using HarmonyLib;
using NuclearOption.Networking;

namespace GW_server_plugin.Patches;

/// <summary>
/// Logs sortie status for players.
/// </summary>
[HarmonyPatch(typeof(Player))]
public class PlayerPatches
{
    /// <summary>
    /// Patch for when a player gets into a plane.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="airframe"></param>
    [HarmonyPrefix] 
    [HarmonyPatch(nameof(Player.FlyOwnedAirframe))]
    public static void AttachPatch(Player __instance, AircraftDefinition airframe)
    {
        var log = new sortieStatus
        {
            Start = true,
            SteamID = __instance.SteamID,
            PlaneName = airframe.unitName
        };
        GwServerPlugin.GrpcMgr.Client?.SendSortieChangeAsync(log);
    }
    
    /// <summary>
    /// Patch for when a player gets gracefully out of a plane.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="airframe"></param>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Player.RecoverAirframeInUse))]
    public static void RecoverPatch(Player __instance, AircraftDefinition airframe)
    {
        var log = new sortieStatus
        {
            Start = false,
            SteamID = __instance.SteamID,
            Killed = false
        };
        GwServerPlugin.GrpcMgr.Client?.SendSortieChangeAsync(log);
    }
}