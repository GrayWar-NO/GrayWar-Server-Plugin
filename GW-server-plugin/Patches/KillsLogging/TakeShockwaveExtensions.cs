using UnityEngine;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace GW_server_plugin.Patches.KillsLogging;

public static class TakeShockwaveExtensions
{
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
