using HarmonyLib;
using NuclearOption.Networking;

namespace GW_server_plugin.Patches.FindWeaponsForKills;
/// <summary>
///     Patch for enabling weapon logs on missiles
/// </summary>
[HarmonyPatch(typeof(Missile))]
public class MissilePatch
{// TODO test: unsure about shockwave behaviour: Update() may not occur while Detonate() is still ongoing.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Missile), nameof(Missile.UserCode_RpcDetonate_897349600))]
    private static void Prefix(Missile __instance)
    {
        var ownerPlayer = __instance.owner.GetPlayer();
        if (ownerPlayer == null) return;
        
        MunitionContext.CurrentOwner = ownerPlayer;
        MunitionContext.CurrentWeaponInfo = __instance.info;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Missile), nameof(Missile.UserCode_RpcDetonate_897349600))]
    private static void Postfix()
    {
        MunitionContext.CurrentOwner = null;
        MunitionContext.CurrentWeaponInfo = null;
    }

}