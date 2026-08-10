using GW_server_plugin.Features;
using HarmonyLib;
using NuclearOption.Networking;
using NuclearOption.SavedMission;

namespace GW_server_plugin.Patches;

/// <summary>
/// Patches mission loading to randomize the weather
/// </summary>
[HarmonyPatch(typeof(NetworkManagerNuclearOption))]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
public class ServerMissionStartPlayerPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(NetworkManagerNuclearOption.ServerMissionStartPlayer))]
    private static void Postfix(
        ref Mission mission,
        ref Player player
        )
    {
        if (RankCatchUpService.RankCatchUp.Value) RankCatchUpService.CatchUpPlayer(player);
    }

}