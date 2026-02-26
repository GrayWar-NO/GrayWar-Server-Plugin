using System;
using GW_server_plugin.Features;
using HarmonyLib;
using NuclearOption.SavedMission;

namespace GW_server_plugin.Patches;



[HarmonyPatch(typeof(MissionSaveLoad))]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
public class MissionSaveLoadPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MissionSaveLoad.TryReadJson))]
    private static void TryReadJsonPrefix(ref string json)
    {
        json = WeatherRandomizerService.RandomizeWeather(json);
    }
}