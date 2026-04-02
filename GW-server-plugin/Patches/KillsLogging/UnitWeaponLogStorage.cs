using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace GW_server_plugin.Patches.KillsLogging;

/// <summary>
///  State class that will be attached to units to log weapon credits.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class UnitWeaponLogState
{
    /// <summary>
    /// Credit per weapon and per player.
    /// </summary>
    public readonly Dictionary<PersistentID, Dictionary<string, float>> WeaponCredit = new();
}

/// <summary>
///  Storage class to attach weapon logs to units.
/// </summary>
public class UnitWeaponLogStorage
{
    private readonly ConditionalWeakTable<Unit, UnitWeaponLogState> _table = new();

    /// <summary>
    /// Get the weapon storage for an unit.
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public UnitWeaponLogState Get(Unit unit)
    {
        return _table.GetOrCreateValue(unit);
    }
}
