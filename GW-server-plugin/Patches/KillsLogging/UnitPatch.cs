using System.Collections.Generic;
using System.Linq;
using GW_server_plugin.Enums;
using GW_server_plugin.Features.IPC.Packets;
using HarmonyLib;
using Newtonsoft.Json;
using NuclearOption.Networking;
using UnityEngine;

namespace GW_server_plugin.Patches.KillsLogging;

/// <summary>
/// Class for logging kills
/// </summary>
[HarmonyPatch(typeof(Unit))]
public class UnitPatch
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="__instance"></param>
    /// <returns></returns>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Unit.ReportKilled))]
    public static bool ReportKilledPrefix(Unit __instance)
    {
        WeaponLoggingExtensions.ReportKilled(__instance);
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="lastDamagedBy"></param>
    /// <param name="damageAmount"></param>
    /// <returns></returns>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Unit.RecordDamage))]
    public static bool RecordDamagePrefix(Unit __instance, PersistentID lastDamagedBy, float damageAmount)
    {
        GwServerPlugin.Logger.LogError("THIS SHOULD NEVER HAPPEN: Original Unit.RecordDamage was called.");
        return true;
    }
}
