using GW_server_plugin.Events;
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
        if (mission == null) return;
        GwServerPlugin.WeatherRandomizer.Apply(ref mission);
        ForceLowWreckDespawn(ref mission);
        MissionEvents.OnMissionLoad(mission);
    }
    
    private static void ForceLowWreckDespawn(ref Mission mission)
    {
        if (!PluginConfig.ForceLowWreckDespawn!.Value) return;        
        mission.missionSettings.wrecksMaxNumber = PluginConfig.MaxWrecks!.Value;
        mission.missionSettings.wrecksDecayTime = PluginConfig.WrecksDecay!.Value;
    }
}