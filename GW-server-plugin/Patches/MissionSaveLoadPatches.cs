using System;
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
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MissionSaveLoad.TryReadJson))]
    private static void TryReadJsonPrefix(ref string json)
    {
        if (!PluginConfig.EnableWeatherRandomizer!.Value) return;
        json = WeatherRandomizerService.RandomizeWeather(json);
    }
}