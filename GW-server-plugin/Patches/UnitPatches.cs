using System.Runtime.CompilerServices;
using GW_server_plugin.Enums;
using GW_server_plugin.Features;
using GW_server_plugin.Features.IPC.Packets;
using HarmonyLib;
using Newtonsoft.Json;

namespace GW_server_plugin.Patches;


/// <summary>
/// Patches for when an unit takes damage. 
/// </summary>
[HarmonyPatch(typeof(Unit))]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
public class UnitPatches
{
    /// <summary>
    /// Postfix for detecting firing at teammates.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="lastDamagedBy"></param>
    /// <param name="damageAmount"></param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Unit.RecordDamage))]
    public static void DetectHitTeamMate(Unit __instance, PersistentID lastDamagedBy, float damageAmount)
    {
        var hitID = __instance.persistentID;
        // return if either hit or damager cannot be found in unit registry.
        if (!(UnitRegistry.TryGetPersistentUnit(hitID, out var hitUnit) && UnitRegistry.TryGetPersistentUnit(lastDamagedBy, out var damagerUnit))) return;
        // return if either hit or damager are not a player.
        if (hitUnit.player is null || damagerUnit.player is null) return;
        // warn if both players on the same team.
        if (hitUnit.unit.NetworkHQ != damagerUnit.unit.NetworkHQ) return;
        ChatService.SendPrivateChatMessage($"You have been warned for damaging teammate {hitUnit.player.PlayerName}", damagerUnit.player);
        var warnLogPacket = new LogEntryPacket
        {
            LogText = $"{damagerUnit.player.SteamID}:{hitUnit.player.SteamID}",
            Channel = LogChannel.Teamkill
        };
        GwServerPlugin.SocketOutBox.Add(JsonConvert.SerializeObject(warnLogPacket));
    }
}

//Unit.RegisterHit for cannon hits ONLY.
//Unit.HitOnPhysicsFrame idem
//Unit.RecordDamage records damage on self.
//Unit.ReportKilled reports self killed
//FireControl.Fire fires a weapon.
//WeaponStation.LaunchMount (owner target)??
//WeaponStation.Fire (owner target)??
//WeaponStation.RemoteFireAuto (owner target)??

//Aircraft.CmdLaunchMissile??


//UnitPart.TakeDamage()!!!!

