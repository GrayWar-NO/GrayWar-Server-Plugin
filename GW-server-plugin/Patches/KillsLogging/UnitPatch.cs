using System.Runtime.CompilerServices;
using HarmonyLib;

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
        __instance.RecordDamage(lastDamagedBy, damageAmount, "Unknown modded way of killing");
        return false;
    }
    
    /// <summary>
    /// Original recordDamage for calling without triggering the prefix.
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="lastDamagedBy"></param>
    /// <param name="damageAmount"></param>
    [HarmonyPatch(nameof(Unit.RecordDamage))]
    [HarmonyReversePatch]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void OriginalRecordDamage(Unit instance, PersistentID lastDamagedBy, float damageAmount)
    {
        
    }
    
}
