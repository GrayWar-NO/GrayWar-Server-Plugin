using HarmonyLib;

namespace GW_server_plugin.Patches.KillsLogging;

/// <summary>
/// Patch to Shockwave in order to change the Update method
/// </summary>
[HarmonyPatch(typeof(Shockwave))]
public class ShockwavePatch
{
    /// <summary>
    /// Shockwave prefix.
    /// </summary>
    /// <param name="__instance"></param>
    /// <returns></returns>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Shockwave.Update))]
    public static bool UpdatePrefix(Shockwave __instance)
    {
        MissileExtensions.Update(__instance);
        return false;
    }
}
