using System;
using NuclearOption.Networking;

namespace GW_server_plugin.Patches.FindWeaponsForKills;

/// <summary>
///     Context info for tracking fired bullets
/// </summary>
public static class MunitionContext
{
    /// <summary>
    ///     Owner of the bullet
    /// </summary>
    [ThreadStatic]
    public static Player? CurrentOwner;

    /// <summary>
    ///     WeaponInfo for the fired bullet
    /// </summary>
    [ThreadStatic]
    public static WeaponInfo? CurrentWeaponInfo;
}