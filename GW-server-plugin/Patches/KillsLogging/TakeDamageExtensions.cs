using System.Xml.Schema;
using NuclearOption.SavedMission;
using UnityEngine;

namespace GW_server_plugin.Patches.KillsLogging;

/// <summary>
/// Class with all the TakeDamage replacements.
/// </summary>
public static class TakeDamageExtensions
{
    /// <summary>
    /// Generic TakeDamage function that sorts itself out for any IDamageable implementation.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="pierceDamage"></param>
    /// <param name="blastDamage"></param>
    /// <param name="amountAffected"></param>
    /// <param name="fireDamage"></param>
    /// <param name="impactDamage"></param>
    /// <param name="dealerID"></param>
    /// <param name="weaponName"></param>
    public static void TakeDamage(
        this IDamageable component,
        float pierceDamage,
        float blastDamage,
        float amountAffected,
        float fireDamage,
        float impactDamage,
        PersistentID dealerID,
        string weaponName)
    {
        switch (component)
        {
            case Missile missile:
                missile.TakeDamage(pierceDamage, blastDamage, amountAffected, fireDamage, impactDamage, dealerID,
                    weaponName);
                break;
            case MountedCargo cargo:
                cargo.TakeDamage(pierceDamage, blastDamage, amountAffected, fireDamage, impactDamage, dealerID,
                    weaponName);
                break;
            case Turbofan turbofan:
                turbofan.TakeDamage(pierceDamage, blastDamage, amountAffected, fireDamage, impactDamage, dealerID,
                    weaponName);
                break;
            case UnitPart part:
                part.TakeDamage(pierceDamage, blastDamage, amountAffected, fireDamage, impactDamage, dealerID,
                    weaponName);
                break;
            default:
                component.TakeDamage(pierceDamage, blastDamage, amountAffected, fireDamage, impactDamage, dealerID);
                break;
        }
    }

    private static void TakeDamage(
        this Missile missile,
        float pierceDamage,
        float blastDamage,
        float amountAffected,
        float fireDamage,
        float impactDamage,
        PersistentID dealerID,
        string weaponName)
    {
        if (missile.disabled)
            return;
        if (impactDamage > 0.0)
        {
            missile.Detonate(-missile.rb.velocity, false, false);
            NetworkSceneSingleton<MessageManager>.i.RpcBombFailMessage(missile.persistentID, impactDamage / 9.81f);
        }

        if (dealerID == missile.persistentID ||
            dealerID == missile.ownerID ||
            dealerID.NotValid ||
            missile.disabled ||
            pierceDamage <= missile.armorProperties.pierceArmor &&
            blastDamage <= missile.armorProperties.blastArmor &&
            fireDamage <= missile.armorProperties.fireArmor)
            return;
        missile.hitpoints -=
            Mathf.Max(pierceDamage - missile.armorProperties.pierceArmor, 0.0f) /
            Mathf.Max(missile.armorProperties.pierceTolerance, 0.1f) +
            Mathf.Max(blastDamage - missile.armorProperties.blastArmor, 0.0f) * amountAffected /
            Mathf.Max(missile.armorProperties.blastTolerance, 0.1f) +
            Mathf.Max(fireDamage - missile.armorProperties.fireArmor, 0.0f) /
            Mathf.Max(missile.armorProperties.fireTolerance, 0.1f);
        if (missile.hitpoints > 0.0)
            return;
        if (UnitRegistry.TryGetPersistentUnit(dealerID, out var dealerUnit) &&
            dealerUnit.GetHQ() != missile.NetworkHQ)
        {
            missile.RecordDamage(dealerID, 1000f, weaponName);
            missile.ReportKilled();
        }

        missile.Detonate(missile.rb.velocity, false, false);
    }

    private static void TakeDamage(
        this Turbofan turbofan,
        float pierceDamage,
        float blastDamage,
        float amountAffected,
        float fireDamage,
        float impactDamage,
        PersistentID dealerID,
        string weaponName)
    {
        if (!turbofan.aircraft.IsServer)
        {
            Debug.LogWarning($"TakeDamage called on {turbofan} but it is not spawned on server");
        }
        else
        {
            var pierceAfterArmor = Mathf.Max(pierceDamage - turbofan.armorProperties.pierceArmor, 0.0f) / Mathf.Max(turbofan.armorProperties.pierceTolerance, 0.01f);
            var blastAfterArmor = blastDamage * amountAffected / Mathf.Max(turbofan.armorProperties.blastTolerance, 0.01f);
            var fireAfterArmor = Mathf.Max(fireDamage - turbofan.armorProperties.fireArmor, 0.0f) / Mathf.Max(turbofan.armorProperties.fireTolerance, 0.01f);
            var totalDmg = pierceAfterArmor + blastAfterArmor + fireAfterArmor + impactDamage;
            if (totalDmg <= 0.0 || turbofan.aircraft == null)
                return;
            if (dealerID.IsValid && dealerID != turbofan.aircraft.persistentID)
                turbofan.aircraft.RecordDamage(dealerID, totalDmg, weaponName);
            turbofan.aircraft.RpcDamage(turbofan.damageIndex, new DamageInfo(pierceAfterArmor, blastAfterArmor, fireAfterArmor, impactDamage));
        }
   
    }

    private static void TakeDamage(
        this MountedCargo cargo,
        float pierceDamage,
        float blastDamage,
        float amountAffected,
        float fireDamage,
        float impactDamage,
        PersistentID dealerID,
        string weaponName)
    {
        var pierceAfterArmour = Mathf.Max(pierceDamage - cargo.armorProperties.pierceArmor, 0.0f) /
                                Mathf.Max(cargo.armorProperties.pierceTolerance, 0.01f);
        var blastAfterArmour = Mathf.Max(blastDamage - cargo.armorProperties.blastArmor, 0.0f) * amountAffected /
                               Mathf.Max(cargo.armorProperties.blastTolerance, 0.01f);
        var fireAfterArmour = Mathf.Max(fireDamage - cargo.armorProperties.fireArmor, 0.0f) /
                              Mathf.Max(cargo.armorProperties.fireTolerance, 0.01f);
        var totalDamageAmount = pierceAfterArmour + blastAfterArmour + fireAfterArmour + impactDamage;
        if (cargo.attachedUnit == null || totalDamageAmount <= 0.0)
            return;
        if (dealerID.IsValid && dealerID != cargo.attachedUnit.persistentID)
            cargo.attachedUnit.RecordDamage(dealerID, totalDamageAmount, weaponName);
        if (!cargo.id.HasValue)
            return;
        cargo.attachedUnit.RpcDamage(cargo.id.Value,
            new DamageInfo(pierceAfterArmour, blastAfterArmour, fireAfterArmour, impactDamage));
    }

    private static void TakeDamage(
        this UnitPart part,
        float pierceDamage,
        float blastDamage,
        float amountAffected,
        float fireDamage,
        float impactDamage,
        PersistentID dealerID,
        string weaponName)
    {
        if (part.parentUnit.SavedUnit is SavedScenery { indestructible: true })
            return;
        if (!part.parentUnit.IsServer)
        {
            Debug.LogWarning((object)$"TakeDamage called on {part} but it is not spawned on server");
        }
        else
        {
            var pierceAfterArmour = Mathf.Max(pierceDamage - part.armorProperties.pierceArmor, 0.0f) /
                                    Mathf.Max(part.armorProperties.pierceTolerance, 0.01f);
            var blastAfterArmour = blastDamage * amountAffected / Mathf.Max(part.armorProperties.blastTolerance, 0.01f);
            var fireAfterArmour = Mathf.Max(fireDamage - part.armorProperties.fireArmor, 0.0f) /
                                  Mathf.Max(part.armorProperties.fireTolerance, 0.01f);
            var damageAmount = pierceAfterArmour + blastAfterArmour + fireAfterArmour + impactDamage;
            if (part.parentUnit == null || damageAmount <= 0.0)
                return;
            if (dealerID.IsValid && dealerID != part.parentUnit.persistentID)
                part.parentUnit.RecordDamage(dealerID, damageAmount, weaponName);
            if (part.criticalPart &&
                part.hitPoints - damageAmount <= 0.0 &&
                !part.parentUnit.disabled)
            {
                part.parentUnit.Networkdisabled = true;
                if (part.parentUnit is not Scenery)
                    part.parentUnit.ReportKilled();
            }

            part.parentUnit.RpcDamage(part.id,
                new DamageInfo(pierceAfterArmour, blastAfterArmour, fireAfterArmour, impactDamage));
            if (part.attachInfo == null ||
                part.attachInfo.detachedFromParentPart ||
                (double)part.hitPoints - damageAmount >= part.structuralThreshold
                || part is AeroPart)
                return;
            part.attachInfo.detachedFromParentPart = true;
            part.parentUnit.DetachPart(part.id,
                part.rb != null ? part.rb.GetPointVelocity(part.xform.position) : Vector3.zero,
                part.xform.position - part.attachInfo.parentPart.transform.position);
        }
    }
}
