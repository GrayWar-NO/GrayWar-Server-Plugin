using GW_server_plugin.Enums;
using GW_server_plugin.Features.IPC.Packets;
using HarmonyLib;
using Newtonsoft.Json;
using NuclearOption.Networking;

namespace GW_server_plugin.Patches.FindWeaponsForKills;

/// <summary>
///     Patches the ReportKilled method, enabling logging of weapon types for kills.
/// </summary>
[HarmonyPatch(typeof(Unit), nameof(Unit.ReportKilled))]
public class ReportKilledPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Unit), nameof(Unit.ReportKilled))]
    private static void Prefix(Unit __instance)
    {
        if (MunitionContext.CurrentOwner is null) return;
        var player = __instance.GetPlayer();
        var outText = player?.SteamID.ToString() ?? __instance.unitName;

        var killLogPacket = new LogEntryPacket
        {
            Channel = LogChannel.Kill,
            LogText =
                $"{MunitionContext.CurrentOwner.SteamID}:{MunitionContext.CurrentWeaponInfo!.weaponName}:{outText}"
        };
        GwServerPlugin.SocketOutBox.Enqueue(JsonConvert.SerializeObject(killLogPacket));
    }
}