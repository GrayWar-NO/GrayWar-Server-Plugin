using System.Runtime.CompilerServices;

namespace GW_server_plugin.Patches.KillsLogging;

/// <summary>
///  State class that will be attached to shockwaves to log their weapon type.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class ShockwaveWeaponTypeLog
{
    /// <summary>
    /// Name of the weapon that triggered the shockwave.
    /// </summary>
    public string WeaponName = "";
}

/// <summary>
/// storage class for attaching weapon types to shockwaves
/// </summary>
public class ShockwaveWeaponTypeStorage
{
    private readonly ConditionalWeakTable<Shockwave, ShockwaveWeaponTypeLog> _table = new();

    /// <summary>
    /// Get the weapon storage for an unit.
    /// </summary>
    /// <param name="shockwave"></param>
    /// <returns></returns>
    public ShockwaveWeaponTypeLog Get(Shockwave shockwave)
    {
        return _table.GetOrCreateValue(shockwave);
    }
}