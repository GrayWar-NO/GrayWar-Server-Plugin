using GW_server_plugin.Helpers;
using HarmonyLib;
using UnityEngine;

namespace GW_server_plugin.Patches;


[HarmonyPatch(typeof(LevelInfo))]
public class LevelInfoLoadPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(LevelInfo.Update))]
    public static void UpdatePostfix(LevelInfo __instance)
    {
        if (GameManager.ShowEffects) return;

        if (__instance.moonPhase >= 0.0)
        {
            __instance.NetworkmoonPhase = __instance.moonPhase + (float) (__instance.timeFactor * (double) Time.deltaTime * 1.1574074051168282E-05);
            if (__instance.moonPhase > 28.0)
                __instance.NetworkmoonPhase = __instance.moonPhase - 28f;
            __instance.SetMoonPhase(__instance.moonPhase);
        }
        
        if (!Globals.NetworkManagerNuclearOptionInstance.Server.Active) return;

        __instance.NetworktimeOfDay = __instance.timeOfDay + (float) (__instance.timeFactor * (double) Time.deltaTime * 0.00027777778450399637);
        if (__instance.timeOfDay > 24.0)
            __instance.NetworktimeOfDay = __instance.timeOfDay - 24f;
        
    }
}