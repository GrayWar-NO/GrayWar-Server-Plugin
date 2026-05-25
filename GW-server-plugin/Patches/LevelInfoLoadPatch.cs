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
        __instance.NetworktimeOfDay = __instance.timeOfDay + (float) (__instance.timeFactor * (double) Time.deltaTime * 0.00027777778450399637);
        if (__instance.timeOfDay > 24.0)
            __instance.NetworktimeOfDay = __instance.timeOfDay - 24f;
        
    }
}