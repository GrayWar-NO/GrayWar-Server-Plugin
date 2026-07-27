using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace GW_server_plugin.Patches.KillsLogging;

/// <summary>
/// Generic transpiler that changes the behaviour of any calls.
/// </summary>
public static class GenericTranspiler
{
    /*
    static readonly MethodInfo Original =
        AccessTools.Method(typeof(Unit), nameof(Unit.RecordDamage), [typeof(PersistentID), typeof(float)]);

    static readonly MethodInfo Replacement =
        AccessTools.Method(typeof(WeaponLoggingExtensions), nameof(WeaponLoggingExtensions.RecordDamage));
    */
    /// <summary>
    /// 
    /// </summary>
    /// <param name="instructions"></param>
    /// <param name="loadWeaponName"></param>
    /// <param name="original"></param>
    /// <param name="replacement"></param>
    /// <returns></returns>
    public static IEnumerable<CodeInstruction> Inject(
        IEnumerable<CodeInstruction> instructions,
        IEnumerable<CodeInstruction> loadWeaponName,
        MethodInfo original,
        MethodInfo replacement)
    {
        var found = false;
        var loader = loadWeaponName.ToList();
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(original))
            {
                found = true;
                // stack before:
                // Unit, PersistentID, float

                foreach (var emit in loader)
                    yield return emit.Clone();

                // stack after:
                // Unit, PersistentID, float, string

                yield return new CodeInstruction(OpCodes.Call, replacement);
            }
            else
            {
                yield return instruction;
            }
        }
        if (!found) GwServerPlugin.Logger.LogError($"FAILED TO FIND TRANSPILE TARGET FOR {original}");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instructions"></param>
    /// <param name="weaponName"></param>
    /// <param name="original"></param>
    /// <param name="replacement"></param>
    /// <returns></returns>
    public static IEnumerable<CodeInstruction> Inject(
        IEnumerable<CodeInstruction> instructions,
        string weaponName,
        MethodInfo original,
        MethodInfo replacement)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(original))
            {
                // stack currently:
                // Unit, PersistentID, float

                yield return new CodeInstruction(OpCodes.Ldstr, weaponName);

                // stack becomes:
                // Unit, PersistentID, float, string

                yield return new CodeInstruction(OpCodes.Call, replacement);
            }
            else
            {
                yield return instruction;
            }
        }
    }
}

/// <summary>
/// Instance transpiler that changes the behaviour of TakeDamage calls.
/// </summary>
public static class TakeDamageTranspiler
{
    private static readonly Type[] OriginalParameters =
    [
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(PersistentID)
    ];

    private static readonly Type[] NewParameters =
    [
        typeof(IDamageable),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(PersistentID),
        typeof(string)
    ];

    private static readonly MethodInfo Original =
        AccessTools.Method(typeof(IDamageable), nameof(IDamageable.TakeDamage), OriginalParameters);

    private static readonly MethodInfo Replacement =
        AccessTools.Method(typeof(TakeDamageExtensions), nameof(TakeDamageExtensions.TakeDamage), NewParameters);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instructions"></param>
    /// <param name="loadWeaponName"></param>
    /// <returns></returns>
    public static IEnumerable<CodeInstruction> Inject(
        IEnumerable<CodeInstruction> instructions,
        IEnumerable<CodeInstruction> loadWeaponName)
    {
        var found = false;
        var loader = loadWeaponName.ToList();
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(Original))
            {
                found = true;
                // stack before:
                // Unit, PersistentID, float

                foreach (var emit in loader)
                    yield return emit.Clone();

                // stack after:
                // Unit, PersistentID, float, string

                yield return new CodeInstruction(OpCodes.Call, Replacement);
            }
            else
            {
                yield return instruction;
            }
        }
        if (!found) GwServerPlugin.Logger.LogError($"FAILED TO FIND TRANSPILE TARGET FOR {Original}");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instructions"></param>
    /// <param name="weaponName"></param>
    /// <returns></returns>
    public static IEnumerable<CodeInstruction> Inject(
        IEnumerable<CodeInstruction> instructions,
        string weaponName)
    {
        var found = false;
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(Original))
            {
                found = true;
                // stack currently:
                // Unit, PersistentID, float

                yield return new CodeInstruction(OpCodes.Ldstr, weaponName);

                // stack becomes:
                // Unit, PersistentID, float, string

                yield return new CodeInstruction(OpCodes.Call, Replacement);
            }
            else
            {
                yield return instruction;
            }
        }
        if (!found) GwServerPlugin.Logger.LogError($"FAILED TO FIND TRANSPILE TARGET FOR {Original}");
    }
}

/// <summary>
/// Instance transpiler that changes the behaviour of ArmorPenetrate calls.
/// </summary>
public static class ArmorPenetrateTranspiler
{
    private static readonly Type[] OriginalParameters =
    [
        typeof(Vector3),
        typeof(Vector3),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(PersistentID)
    ];

    private static readonly Type[] NewParameters =
    [
        typeof(Vector3),
        typeof(Vector3),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(PersistentID),
        typeof(string)
    ];

    private static readonly MethodInfo Original =
        AccessTools.Method(typeof(DamageEffects), nameof(DamageEffects.ArmorPenetrate), OriginalParameters);

    private static readonly MethodInfo Replacement =
        AccessTools.Method(typeof(DamageEffectExtensions), nameof(DamageEffectExtensions.ArmorPenetrate),
            NewParameters);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instructions"></param>
    /// <param name="loadWeaponName"></param>
    /// <returns></returns>
    public static IEnumerable<CodeInstruction> Inject(
        IEnumerable<CodeInstruction> instructions,
        IEnumerable<CodeInstruction> loadWeaponName)
    {
        var found = false;
        var loader = loadWeaponName.ToList();
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(Original))
            {
                found = true;
                // stack before:
                // Unit, PersistentID, float

                foreach (var emit in loader)
                    yield return emit.Clone();

                // stack after:
                // Unit, PersistentID, float, string

                yield return new CodeInstruction(OpCodes.Call, Replacement);
            }
            else
            {
                yield return instruction;
            }
        }
        if (!found) GwServerPlugin.Logger.LogError($"FAILED TO FIND TRANSPILE TARGET FOR {Original}");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instructions"></param>
    /// <param name="weaponName"></param>
    /// <returns></returns>
    public static IEnumerable<CodeInstruction> Inject(
        IEnumerable<CodeInstruction> instructions,
        string weaponName)
    {
        var found = true;
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(Original))
            {
                found = false;
                // stack currently:
                // Unit, PersistentID, float

                yield return new CodeInstruction(OpCodes.Ldstr, weaponName);

                // stack becomes:
                // Unit, PersistentID, float, string

                yield return new CodeInstruction(OpCodes.Call, Replacement);
            }
            else
            {
                yield return instruction;
            }
        }
        if (!found) GwServerPlugin.Logger.LogError($"FAILED TO FIND TRANSPILE TARGET FOR {Original}");
    }
}

/// <summary>
/// 
/// </summary>
public static class RecordDamageTranspiler
{
    private static readonly MethodInfo Original =
        AccessTools.Method(typeof(Unit), nameof(Unit.RecordDamage), [typeof(PersistentID), typeof(float)]);

    private static readonly MethodInfo Replacement =
        AccessTools.Method(typeof(WeaponLoggingExtensions), nameof(WeaponLoggingExtensions.RecordDamage));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instructions"></param>
    /// <param name="loadWeaponName"></param>
    /// <returns></returns>
    public static IEnumerable<CodeInstruction> Inject(
        IEnumerable<CodeInstruction> instructions,
        IEnumerable<CodeInstruction> loadWeaponName)
    {
        var loader = loadWeaponName.ToList();
        var found = false;
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(Original))
            {
                found = true;
                // stack before:
                // Unit, PersistentID, float

                foreach (var emit in loader)
                    yield return emit.Clone();

                // stack after:
                // Unit, PersistentID, float, string

                yield return new CodeInstruction(OpCodes.Call, Replacement);
            }
            else
            {
                yield return instruction;
            }
        }
        if (!found) GwServerPlugin.Logger.LogError($"FAILED TO FIND TRANSPILE TARGET FOR {Original}");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instructions"></param>
    /// <param name="weaponName"></param>
    /// <returns></returns>
    public static IEnumerable<CodeInstruction> Inject(
        IEnumerable<CodeInstruction> instructions,
        string weaponName)
    {
        var found = false;
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(Original))
            {
                found = true;
                // stack currently:
                // Unit, PersistentID, float
                yield return new CodeInstruction(OpCodes.Ldstr, weaponName);

                // stack becomes:
                // Unit, PersistentID, float, string

                yield return new CodeInstruction(OpCodes.Call, Replacement);
            }
            else
            {
                yield return instruction;
            }
        }
        if (!found) GwServerPlugin.Logger.LogError($"FAILED TO FIND TRANSPILE TARGET FOR {Original}");
    }
}

/// <summary>
/// 
/// </summary>
public static class BlastFragTranspiler
{
    private static readonly MethodInfo Original =
        AccessTools.Method(typeof(DamageEffects), nameof(DamageEffects.BlastFrag));

    private static readonly MethodInfo Replacement =
        AccessTools.Method(typeof(DamageEffectExtensions), nameof(DamageEffectExtensions.BlastFrag));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instructions"></param>
    /// <param name="loadWeaponName"></param>
    /// <returns></returns>
    public static IEnumerable<CodeInstruction> Inject(
        IEnumerable<CodeInstruction> instructions,
        IEnumerable<CodeInstruction> loadWeaponName)
    {
        var found = false;
        var loader = loadWeaponName.ToList();
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(Original))
            {
                found = true;
                // stack before:
                // Unit, PersistentID, float

                foreach (var emit in loader)
                {
                    yield return emit.Clone();
                }
                // stack after:
                // Unit, PersistentID, float, string

                yield return new CodeInstruction(OpCodes.Call, Replacement);
            }
            else
            {
                yield return instruction;
            }
        }
        if (!found) GwServerPlugin.Logger.LogError($"FAILED TO FIND TRANSPILE TARGET FOR {Original}");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instructions"></param>
    /// <param name="weaponName"></param>
    /// <returns></returns>
    public static IEnumerable<CodeInstruction> Inject(
        IEnumerable<CodeInstruction> instructions,
        string weaponName)
    {
        var found = false;
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(Original))
            {
                // stack currently:
                // Unit, PersistentID, float
                found = true;
                yield return new CodeInstruction(OpCodes.Ldstr, weaponName);

                // stack becomes:
                // Unit, PersistentID, float, string

                yield return new CodeInstruction(OpCodes.Call, Replacement);
            }
            else
            {
                yield return instruction;
            }
        }
        if (!found) GwServerPlugin.Logger.LogError($"FAILED TO FIND TRANSPILE TARGET FOR {Original}");
    }
}

/// <summary>
/// Instance transpiler that changes the behaviour of TakeShockwave calls.
/// </summary>
public static class TakeShockwaveTranspiler
{
    private static readonly Type[] OriginalParameters =
    [
        typeof(Vector3),
        typeof(float),
        typeof(float)
    ];

    private static readonly Type[] NewParameters =
    [
        typeof(IDamageable),
        typeof(Vector3),
        typeof(float),
        typeof(float),
        typeof(string)
    ];

    private static readonly MethodInfo Original =
        AccessTools.Method(typeof(IDamageable), nameof(IDamageable.TakeShockwave), OriginalParameters);

    private static readonly MethodInfo Replacement =
        AccessTools.Method(typeof(TakeShockwaveExtensions), nameof(TakeShockwaveExtensions.TakeShockwave),
            NewParameters);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instructions"></param>
    /// <param name="loadWeaponName"></param>
    /// <returns></returns>
    public static IEnumerable<CodeInstruction> Inject(
        IEnumerable<CodeInstruction> instructions,
        IEnumerable<CodeInstruction> loadWeaponName)
    {
        var loader = loadWeaponName.ToList();
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(Original))
            {
                // stack before:
                // Unit, PersistentID, float

                foreach (var emit in loader)
                    yield return emit.Clone();

                // stack after:
                // Unit, PersistentID, float, string

                yield return new CodeInstruction(OpCodes.Call, Replacement);
            }
            else
            {
                yield return instruction;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instructions"></param>
    /// <param name="weaponName"></param>
    /// <returns></returns>
    public static IEnumerable<CodeInstruction> Inject(
        IEnumerable<CodeInstruction> instructions,
        string weaponName)
    {
        var found = false;
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(Original))
            {
                found = true;
                yield return new CodeInstruction(OpCodes.Ldstr, weaponName);
                yield return new CodeInstruction(OpCodes.Call, Replacement);
            }
            else
            {
                yield return instruction;
            }
        }
        if (!found) GwServerPlugin.Logger.LogError($"FAILED TO FIND TRANSPILE TARGET FOR {Original}");
    }
}

/// <summary>
/// Instance transpiler that changes the behaviour of HasShockwaveReached calls.
/// </summary>
public static class HasShockwaveReachedTranspiler
{
    private static readonly Type[] OriginalParameters =
    [
        typeof(Vector3),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(PersistentID)
    ];

    private static readonly Type[] NewParameters =
    [
        typeof(Shockwave.InfluencedObject),
        typeof(Vector3),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(PersistentID),
        typeof(string)
    ];

    private static readonly MethodInfo Original =
        AccessTools.Method(typeof(Shockwave.InfluencedObject), nameof(Shockwave.InfluencedObject.HasShockwaveReached), OriginalParameters);

    private static readonly MethodInfo Replacement =
        AccessTools.Method(typeof(MissileExtensions), nameof(MissileExtensions.HasShockwaveReached),
            NewParameters);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instructions"></param>
    /// <param name="loadWeaponName"></param>
    /// <returns></returns>
    public static IEnumerable<CodeInstruction> Inject(
        IEnumerable<CodeInstruction> instructions,
        IEnumerable<CodeInstruction> loadWeaponName)
    {
        var instrList = instructions.ToList();
        var loader = loadWeaponName.ToList();
        var rtList = new List<CodeInstruction>();
        var found = false;
        foreach (var instruction in instrList)
        {
            if (instruction.Calls(Original))
            {
                found = true;
                rtList.AddRange(loader.Select(emit => emit.Clone()));
                rtList.Add(new CodeInstruction(instruction.opcode, Replacement));
                var index = rtList.Count - 1;
                while (rtList[index].opcode != OpCodes.Ldloca_S) { index--; }
                rtList[index].opcode = OpCodes.Ldloc_S;
            }
            else
            {
                rtList.Add(instruction);
            }
        }
        if (!found) GwServerPlugin.Logger.LogError($"FAILED TO FIND TRANSPILE TARGET FOR {Original}");
        return rtList;
    }
}
