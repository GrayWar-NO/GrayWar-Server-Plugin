using System;
using System.Reflection;
using GW_server_plugin.Features;
using HarmonyLib;
using NuclearOption.SavedMission;

namespace GW_server_plugin.Patches;



/// <summary>
/// Patches mission loading to randomize the weather
/// </summary>
[HarmonyPatch(typeof(MissionSaveLoad))]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
public class MissionSaveLoadPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MissionSaveLoad.TryLoad))]
    private static void Postfix(
        MissionKey item,
        ref Mission mission,
        ref string error,
        ref bool __result)
    {
        if (!__result || mission == null)
            return;
        if (!PluginConfig.EnableWeatherRandomizer!.Value) return;
        var rnd = new Random();
        mission.environment.timeOfDay = rnd.Next(5, 18);
        mission.environment.timeFactor = 8f;
        mission.environment.weatherIntensity = (float)(rnd.NextDouble() * 0.8);
        mission.environment.cloudAltitude = (float)(500 + rnd.NextDouble() * 1000);
        mission.environment.windSpeed = (float)(rnd.NextDouble() * 4);
        mission.environment.windTurbulence = (float)rnd.NextDouble();
        mission.environment.windHeading = rnd.Next(0, 360);
    }
}