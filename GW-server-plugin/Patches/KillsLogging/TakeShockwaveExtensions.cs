using UnityEngine;

namespace GW_server_plugin.Patches.KillsLogging;

/// <summary>
///     Extension class for IDamageable.TakeShockwave
/// </summary>
public static class TakeShockwaveExtensions
{
    /// <summary>
    ///     Replaces <see cref="IDamageable.TakeShockwave"/> taking a weaponName into account.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="origin"></param>
    /// <param name="overpressure"></param>
    /// <param name="blastPower"></param>
    /// <param name="weaponName"></param>
    public static void TakeShockwave(
        this IDamageable component,
        Vector3 origin,
        float overpressure,
        float blastPower,
        string weaponName)
    {
        switch (component)
        {
            case SwashRotor rotor:
                if (overpressure <= (double)rotor.armorProperties.overpressureLimit)
                    return;
                rotor.TakeDamage(0.0f, overpressure - rotor.armorProperties.overpressureLimit, 1f, 0.0f, 0.0f,
                    PersistentID.None, weaponName);
                break;
            case SoftBodyRotor rotor:
                if (overpressure <= (double)rotor.armorProperties.overpressureLimit)
                    return;
                rotor.TakeDamage(0.0f, overpressure - rotor.armorProperties.overpressureLimit, 1f, 0.0f, 0.0f,
                    PersistentID.None, weaponName);
                break;
            default:
                component.TakeShockwave(origin, overpressure, blastPower);
                break;
        }
    }
}
