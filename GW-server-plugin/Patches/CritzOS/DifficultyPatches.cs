using System;
using HarmonyLib;
using NuclearOption.SavedMission;

namespace GW_server_plugin.Patches.CritzOS;



/// <summary>
/// Patches mission loading to randomize the weather
/// </summary>
[HarmonyPatch(typeof(MissionSaveLoad))]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
public class DifficultyPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MissionSaveLoad.TryLoad))]
    private static void Postfix(
        MissionKey item,
        ref Mission? mission,
        ref string error)
    {
        if (mission == null) return;
        
        ModifyDifficulty(ref mission);
    }
    private static void ModifyDifficulty(ref Mission mission)
    {
        // This is set to scale for larger player counts better
        foreach (var f in mission.factions)
        {
            f.addAIPerEnemyPlayer = 0.80f;
            f.AIAircraftLimit = 8;
        }

        mission.missionSettings.nuclearEscalationThreshold =
            Math.Max(mission.missionSettings.nuclearEscalationThreshold, 2100);

        mission.missionSettings.strategicEscalationThreshold =
            Math.Max(mission.missionSettings.strategicEscalationThreshold, 3000);
        GwServerPlugin.Logger.LogInfo("Difficulty Patched");
    }
}