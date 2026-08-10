using System.Linq;
using GW_server_plugin.Events;
using HarmonyLib;
using NuclearOption.SavedMission.Outcomes;

namespace GW_server_plugin.Patches;


/// <summary>
///     Hooks into FactionHQ.DeclareEndGame to detect mission end.
/// </summary>
[HarmonyPatch(typeof(FactionHQ))]
public class MissionEndHook
{
    /// <summary>
    ///     Hooks into FactionHQ.DeclareEndGame to detect mission end.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(FactionHQ.DeclareEndGame))]
    // ReSharper disable once InconsistentNaming
    public static void EndGamePrefix(FactionHQ __instance, EndType endType)
    {
        if (endType == EndType.Victory)
            MissionEvents.OnMissionEnd(__instance);
        else
        {
            // ReSharper disable once InconsistentNaming
            var otherHQ = MissionManager.CurrentMission!.factions
                .Select(faction => faction.FactionHQ)
                .FirstOrDefault(hq => hq != null && hq != __instance)!;
            MissionEvents.OnMissionEnd(otherHQ);
        }
    }
}