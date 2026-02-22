using HarmonyLib;
using NuclearOption.Networking;

namespace GW_server_plugin.Patches.FindWeaponsForKills;

/// <summary>
///     Harmony patches for logging which weapon kills someone.
/// </summary>
[HarmonyPatch(typeof(BulletSim), nameof(BulletSim.Bullet.TrajectoryTrace))]
public class TrajectoryTracePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BulletSim.Bullet), nameof(BulletSim.Bullet.TrajectoryTrace))]
    private static void Prefix(Unit owner, WeaponInfo info)
    {
        var ownerPlayer = owner.GetPlayer();
        if (ownerPlayer == null) return;
        
        MunitionContext.CurrentOwner = ownerPlayer;
        MunitionContext.CurrentWeaponInfo = info;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BulletSim.Bullet), nameof(BulletSim.Bullet.TrajectoryTrace))]
    private static void Postfix()
    {
        MunitionContext.CurrentOwner = null;
        MunitionContext.CurrentWeaponInfo = null;
    }
}