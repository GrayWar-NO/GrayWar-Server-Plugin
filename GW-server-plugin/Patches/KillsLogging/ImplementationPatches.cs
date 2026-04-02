using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

// ReSharper disable UnusedMember.Local

// ReSharper disable ArrangeTypeModifiers

namespace GW_server_plugin.Patches.KillsLogging;

[HarmonyPatch(typeof(ARHSeeker), nameof(ARHSeeker.ARHSeeker_OnJam))]
// ReSharper disable once InconsistentNaming
class PatchARHSeekerOnJam
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return RecordDamageTranspiler.Inject(instructions, "Radar Jamming Pod");
    }
}

[HarmonyPatch(typeof(ARHSeeker), nameof(ARHSeeker.DatalinkMode))]
// ReSharper disable once InconsistentNaming
class PatchARHSeekerDatalinkMode
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var missileField = AccessTools.Field(typeof(ARHSeeker), nameof(ARHSeeker.missile));
        var weaponInfoField = AccessTools.Field(typeof(Missile), nameof(Missile.info));
        var weaponNameField = AccessTools.Field(typeof(WeaponInfo), nameof(WeaponInfo.weaponName));
        var loader = new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldfld, missileField),
            new CodeInstruction(OpCodes.Ldfld, weaponInfoField),
            new CodeInstruction(OpCodes.Ldfld, weaponNameField)
        };
        return RecordDamageTranspiler.Inject(instructions, loader);
    }
}

[HarmonyPatch(typeof(IRSeeker), nameof(IRSeeker.IRLockCheck))]
// ReSharper disable once InconsistentNaming
class PatchIRSeekerLockCheck
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var missileField = AccessTools.Field(typeof(IRSeeker), nameof(IRSeeker.missile));
        var weaponInfoField = AccessTools.Field(typeof(Missile), nameof(Missile.info));
        var weaponNameField = AccessTools.Field(typeof(WeaponInfo), nameof(WeaponInfo.weaponName));
        var loader = new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldfld, missileField),
            new CodeInstruction(OpCodes.Ldfld, weaponInfoField),
            new CodeInstruction(OpCodes.Ldfld, weaponNameField)
        };
        return RecordDamageTranspiler.Inject(instructions, loader);
    }
}

[HarmonyPatch(typeof(SARHSeeker), nameof(SARHSeeker.SARHSeeker_OnJam))]
// ReSharper disable once InconsistentNaming
class PatchSARHSeekerDatalinkMode
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return RecordDamageTranspiler.Inject(instructions, "Radar Jamming Pod");
    }
}

[HarmonyPatch(typeof(Laser), nameof(Laser.FixedUpdate))]
class PatchImpactDetectorFixedUpdate
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var laserWeaponInfo = AccessTools.Field(typeof(Laser), nameof(Laser.info));
        var weaponNameField = AccessTools.Field(typeof(WeaponInfo), nameof(WeaponInfo.weaponName));
        var loader = new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldfld, laserWeaponInfo),
            new CodeInstruction(OpCodes.Ldfld, weaponNameField)
        };

        return TakeDamageTranspiler.Inject(instructions, loader);
    }
}

[HarmonyPatch(typeof(FuelTank), nameof(FuelTank.FuelTankFire))]
class PatchFuelTankFire
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return TakeDamageTranspiler.Inject(instructions, "Fuel tank fire");
    }
}

[HarmonyPatch(typeof(AeroPart), nameof(AeroPart.OnCollisionEnter))]
class PatchCollisionDamage
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return TakeDamageTranspiler.Inject(instructions, "Collision");
    }
}

[HarmonyPatch(typeof(DamageParticles), nameof(DamageParticles.SlowUpdate))]
class PatchSlowFireUpdate
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return TakeDamageTranspiler.Inject(instructions, "Fire");
    }
}

[HarmonyPatch(typeof(BulletSim.Bullet), nameof(BulletSim.Bullet.TrajectoryTrace))]
class PatchBulletTrajectoryTrace
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var weaponNameField = AccessTools.Field(typeof(WeaponInfo), nameof(WeaponInfo.weaponName));

        var loader = new[]
        {
            new CodeInstruction(OpCodes.Ldarg_2),
            new CodeInstruction(OpCodes.Ldfld, weaponNameField)
        };

        var newInstr = TakeDamageTranspiler.Inject(instructions, loader);
        return BlastFragTranspiler.Inject(newInstr, loader);
    }
}

[HarmonyPatch(typeof(ImpactDetector), nameof(ImpactDetector.FixedUpdate))]
class PatchImpactDetectorSlingHookUpdate
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return TakeDamageTranspiler.Inject(instructions, "Sling hook ripped off");
    }
}

[HarmonyPatch(typeof(Missile), nameof(Missile.UserCode_RpcDetonate_897349600))]
class PatchMissileRPCDetonate
{
    private static readonly Type[] OriginalParameters =
    [
        typeof(Rigidbody),
        typeof(PersistentID),
        typeof(Vector3),
        typeof(Vector3),
        typeof(bool),
        typeof(float),
        typeof(bool),
        typeof(bool)
    ];

    private static readonly Type[] NewParameters =
    [
        typeof(Missile.Warhead),
        typeof(Rigidbody),
        typeof(PersistentID),
        typeof(Vector3),
        typeof(Vector3),
        typeof(bool),
        typeof(float),
        typeof(bool),
        typeof(bool),
        typeof(string)
    ];

    private static readonly MethodInfo Original =
        AccessTools.Method(typeof(Missile.Warhead), nameof(Missile.Warhead.Detonate), OriginalParameters);

    private static readonly MethodInfo Replacement =
        AccessTools.Method(typeof(MissileExtensions), nameof(MissileExtensions.Detonate), NewParameters);

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var missileInfo = AccessTools.Field(typeof(Missile), nameof(Missile.info));
        var weaponNameField = AccessTools.Field(typeof(WeaponInfo), nameof(WeaponInfo.weaponName));
        var loader = new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldfld, missileInfo),
            new CodeInstruction(OpCodes.Ldfld, weaponNameField)
        };

        return GenericTranspiler.Inject(instructions, loader, Original, Replacement);
    }
}

[HarmonyPatch]
class PatchUnitHitOnPhysicsFrame
{
    static MethodBase TargetMethod()
    {
        // Get the async state machine generated for HitOnPhysicsFrame
        var method = AccessTools.Method(typeof(Unit), nameof(Unit.HitOnPhysicsFrame));
        var attr = method.GetCustomAttribute<AsyncStateMachineAttribute>();

        return attr == null
            ? throw new Exception("HitOnPhysicsFrame is not an async method")
            :
            // Patch MoveNext instead
            AccessTools.Method(attr.StateMachineType, "MoveNext");
    }


    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var list = new List<CodeInstruction>(instructions);
        LocalBuilder? weaponInfoLocal = null;

        foreach (var instr in list)
        {
            if (instr.opcode != OpCodes.Stloc_S || instr.operand is not LocalBuilder lb) continue;
            if (lb.LocalType != typeof(WeaponInfo)) continue;
            weaponInfoLocal = lb;
            break;
        }

        if (weaponInfoLocal is null) throw new NullReferenceException("No weaponInfo in provided method");


        var weaponNameField = AccessTools.Field(typeof(WeaponInfo), nameof(WeaponInfo.weaponName));
        var loader = new[]
        {
            new CodeInstruction(OpCodes.Ldloc_S, weaponInfoLocal),
            new CodeInstruction(OpCodes.Ldfld, weaponNameField)
        };
        return ArmorPenetrateTranspiler.Inject(list, loader);
    }
}

[HarmonyPatch(typeof(Missile), nameof(Missile.PenetrateObject))]
class PatchMissilePenetrateObject
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var missileWeaponInfo = AccessTools.Field(typeof(Missile), nameof(Missile.info));
        var weaponNameField = AccessTools.Field(typeof(WeaponInfo), nameof(WeaponInfo.weaponName));
        var loader = new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldfld, missileWeaponInfo),
            new CodeInstruction(OpCodes.Ldfld, weaponNameField)
        };

        return ArmorPenetrateTranspiler.Inject(instructions, loader);
    }
}

[HarmonyPatch(typeof(Unit), nameof(Unit.RegisterHit))]
class PatchUnitRegisterHit
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var weaponNameField = AccessTools.Field(typeof(WeaponInfo), nameof(WeaponInfo.weaponName));
        var loader = new[]
        {
            new CodeInstruction(OpCodes.Ldarg_S, 4),
            new CodeInstruction(OpCodes.Ldfld, weaponNameField)
        };

        return ArmorPenetrateTranspiler.Inject(instructions, loader);
    }
}

[HarmonyPatch(typeof(ExplosionTester), nameof(ExplosionTester.Detonate))]
class PatchExplosionTester
{
    [HarmonyPrefix]
    public static bool Detonate(ExplosionTester __instance)
    {
        DamageEffectExtensions.BlastFrag(
            __instance.yieldSlider.value * __instance.yieldSlider.value,
            __instance.explosionPoint.transform.position,
            PersistentID.None,
            PersistentID.None,
            "Explosion tester");
        return false;
    }
}

[HarmonyPatch]
class PatchMissileExplosionForceOnPhysicsFrame
{
    static MethodBase TargetMethod()
    {
        // Get the async state machine generated for HitOnPhysicsFrame
        var method = AccessTools.Method(typeof(Missile), nameof(Missile.ExplosionForceOnPhysicsFrame));
        var attr = method.GetCustomAttribute<AsyncStateMachineAttribute>();

        var tm = attr == null
            ? throw new Exception("ExplosionForceOnPhysicsFrame is not an async method")
            :
            // Patch MoveNext instead
            AccessTools.Method(attr.StateMachineType, "MoveNext");
        
        GwServerPlugin.Logger.LogDebug($"found method {method} for Missile.ExplosionForceOnPhysicsFrame.");
        
        return tm;
        
    }


    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var missileWeaponInfo = AccessTools.Field(typeof(Missile), nameof(Missile.info));
        var weaponNameField = AccessTools.Field(typeof(WeaponInfo), nameof(WeaponInfo.weaponName));

        var loader = new[]
        {
            new CodeInstruction(OpCodes.Ldloc_1),
            new CodeInstruction(OpCodes.Ldfld, missileWeaponInfo),
            new CodeInstruction(OpCodes.Ldfld, weaponNameField)
        };
        return BlastFragTranspiler.Inject(instructions, loader);
    }
}

[HarmonyPatch(typeof(SoftBodyRotor), nameof(SoftBodyRotor.TakeShockwave))]
class PatchSoftBodyTakeShockwave
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return TakeDamageTranspiler.Inject(instructions, "Overpressure damage");
    }
}

[HarmonyPatch(typeof(SwashRotor), nameof(SwashRotor.TakeShockwave))]
class PatchSwashTakeShockwave
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return TakeDamageTranspiler.Inject(instructions, "Overpressure damage");
    }
}

[HarmonyPatch(typeof(Explosion), nameof(Explosion.SimulateForce))]
class PatchExplosionSimForce
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return TakeShockwaveTranspiler.Inject(instructions, "Unknown Codepath Explosion.SimulateForce");
    }
}

