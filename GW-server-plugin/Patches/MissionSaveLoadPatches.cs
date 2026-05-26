using System;
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
        if (!__result) return;
        WeatherRandomizer(ref mission);
        ForceLowWreckDespawn(ref mission);
    }

    private static void WeatherRandomizer(ref Mission mission)
    {
        if (mission == null) return;
        if (!PluginConfig.EnableWeatherRandomizer!.Value) return;
        var rnd = new Random();
        mission.environment.timeOfDay = rnd.Next(2, 10);
        mission.environment.timeFactor = 1f; // TIME FACTOR goes from -2 to 3, values are [0, 0.5, 1, 10, 30, 60]
        mission.environment.weatherIntensity = (float)rnd.NextDouble();
        mission.environment.cloudAltitude = (float)(200 + rnd.NextDouble() * 1000);
        mission.environment.windSpeed = (float)(rnd.NextDouble() * 10); // in m/s (10m/s is 36kph)
        mission.environment.windTurbulence = (float)rnd.NextDouble();
        mission.environment.windHeading = rnd.Next(0, 360); 
        mission.environment.windRandomHeading = rnd.Next(0, 180);
    }

    private static void ForceLowWreckDespawn(ref Mission mission)
    {
        mission.missionSettings.wrecksMaxNumber = 100;
        mission.missionSettings.wrecksDecayTime = 300;
    }
}