using GW_server_plugin.Enums;
using GW_server_plugin.Features.IPC.Packets;
using HarmonyLib;
using Newtonsoft.Json;
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
        var packet = new LogEntryPacket
        {
            Channel = LogChannel.SortieStatus,
            LogText = $"1:{__instance.SteamID}:{airframe.unitName}"
        };
        GwServerPlugin.SocketOutBox.Add(JsonConvert.SerializeObject(packet));
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
        var packet = new LogEntryPacket
        {
            Channel = LogChannel.SortieStatus,
            LogText = $"0:{__instance.SteamID}:1"// GetOut:steamID:Success
        };
        GwServerPlugin.SocketOutBox.Add(JsonConvert.SerializeObject(packet));
    }
}