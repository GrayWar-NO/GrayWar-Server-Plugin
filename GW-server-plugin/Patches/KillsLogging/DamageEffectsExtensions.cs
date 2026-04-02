using NuclearOption.Networking;
using UnityEngine;

namespace GW_server_plugin.Patches.KillsLogging;

/// <summary>
/// Extension functions for the DamageEffects class.
/// </summary>
public static class DamageEffectExtensions
{
    /// <summary>
    /// Replacement for DamageEffects.ArmorPenetrate
    /// </summary>
    /// <param name="position"></param>
    /// <param name="velocity"></param>
    /// <param name="muzzleVelocity"></param>
    /// <param name="pierceDamage"></param>
    /// <param name="blastDamage"></param>
    /// <param name="dealerID"></param>
    /// <param name="weaponName"></param>
    public static void ArmorPenetrate(
        Vector3 position,
        Vector3 velocity,
        float muzzleVelocity,
        float pierceDamage,
        float blastDamage,
        PersistentID dealerID,
        string weaponName)
    {
        var num = 0;
        var start = position;
        var vector3 = velocity;
        if (blastDamage > 0.0)
            BlastFrag(blastDamage, position, dealerID, PersistentID.None, weaponName);
        for (;
             num < 10 && Physics.Linecast(start, start + vector3 * 0.1f, out var hitInfo, -8193) &&
             vector3.sqrMagnitude > muzzleVelocity * (double)muzzleVelocity * 0.10000000149011612;
             start = hitInfo.point + 0.1f * vector3.normalized)
        {
            ++num;
            var component = hitInfo.collider.gameObject.GetComponent<IDamageable>();
            if (component == null)
                break;
            var pierceDamage1 = (float)(Mathf.Max(Vector3.Dot(vector3.normalized, -hitInfo.normal), 0.5f) *
                                        (double)pierceDamage * (vector3.magnitude / (double)muzzleVelocity));
            component.TakeDamage(pierceDamage1, 1f, 0.0f, 0.0f, 0.0f, dealerID, weaponName);
            var armorProperties = component.GetArmorProperties();
            if (pierceDamage1 <= (double)armorProperties.pierceArmor)
                break;
            vector3 *= (pierceDamage1 - armorProperties.pierceArmor * 2f) / pierceDamage1;
        }
    }

    /// <summary>
    /// Replacement for DamageEffects.FragTrace
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="fragVector"></param>
    /// <param name="blastYield"></param>
    /// <param name="blastPower"></param>
    /// <param name="fragTarget"></param>
    /// <param name="dealerID"></param>
    /// <param name="debugTransform"></param>
    /// <param name="weaponName"></param>
    public static void FragTrace(
        Vector3 origin,
        Vector3 fragVector,
        float blastYield,
        float blastPower,
        Collider fragTarget,
        PersistentID dealerID,
        Transform debugTransform,
        string weaponName)
    {
        var distanceTraveled = 0.0f;
        var start = origin;
        var piercedBlastArmor = 0.0f;
        var ctr = 0;
        while (ctr < 10 && Physics.Linecast(start, start + fragVector, out var hitInfo, -40961))
        {
            ++ctr;
            distanceTraveled += hitInfo.distance;
            if (!hitInfo.collider.gameObject.TryGetComponent<IDamageable>(out var hitDamageable))
            {
                if (!(hitInfo.collider.sharedMaterial != GameAssets.i.terrainMaterial) ||
                    !(hitInfo.collider.sharedMaterial != null))
                    break;
                start = hitInfo.point + fragVector.normalized * 0.1f;
            }
            else
            {
                var armorProperties = hitDamageable.GetArmorProperties();
                if (armorProperties == null)
                    break;
                var blastSomething = Mathf.Max((distanceTraveled + piercedBlastArmor) / blastPower, 1f);
                var overpressure = (float)(25000.0 / (blastSomething * (double)blastSomething * blastSomething));
                if (overpressure <= 0.0)
                    break;
                if (hitInfo.collider == fragTarget)
                {
                    double x = hitInfo.collider.bounds.extents.x;
                    var bounds = hitInfo.collider.bounds;
                    double y = bounds.extents.y;
                    var num4 = x + y;
                    bounds = hitInfo.collider.bounds;
                    double z2 = bounds.extents.z;
                    var num5 = (num4 + z2) * 0.33329999446868896;
                    var num6 = (float)(num5 * num5);
                    var num7 = Mathf.Clamp(
                                   Mathf.Max(distanceTraveled * distanceTraveled, blastPower * blastPower) / num6, 0.0f,
                                   10f) /
                               (float)(1.0 + armorProperties.blastArmor * 0.05000000074505806);
                    if (blastPower >= 0.5)
                        hitDamageable.TakeShockwave(origin, overpressure, blastPower, weaponName);
                    var num8 =
                        Mathf.Clamp01((float)((blastPower * 500.0 - armorProperties.blastArmor) /
                                              (armorProperties.blastArmor * 2.0)));
                    var blastDamage = overpressure * num8 - armorProperties.blastArmor;
                    if (blastDamage <= 0.0)
                        break;
                    if (NetworkManagerNuclearOption.i.Server.Active)
                        hitDamageable.TakeDamage(0.0f, blastDamage, Mathf.Clamp01(num7), 0.0f, 0.0f, dealerID,
                            weaponName);
                    if (!PlayerSettings.debugVis)
                        break;
                    var gameObject =
                        NetworkSceneSingleton<Spawner>.i.SpawnLocal(GameAssets.i.debugArrow, debugTransform);
                    gameObject.GetComponent<MeshRenderer>().material
                        .SetColor("_EmissionColor", new Color(1f, 0.5f, 0.0f, 1f));
                    gameObject.transform.position = origin;
                    gameObject.transform.rotation = Quaternion.LookRotation(hitInfo.point - origin);
                    gameObject.transform.localScale = new Vector3(0.5f, 0.5f, distanceTraveled);
                    NetworkSceneSingleton<Spawner>.i.DestroyLocal(gameObject, 10f);
                    break;
                }

                piercedBlastArmor += armorProperties.blastArmor * 0.1f;
                start = hitInfo.point + fragVector.normalized * 0.1f;
            }
        }
    }

    /// <summary>
    ///  Replacement fro BlastFrag
    /// </summary>
    /// <param name="blastYield"></param>
    /// <param name="blastPosition"></param>
    /// <param name="dealerID"></param>
    /// <param name="missileID"></param>
    /// <param name="weaponName"></param>
    public static void BlastFrag(
        float blastYield,
        Vector3 blastPosition,
        PersistentID dealerID,
        PersistentID missileID,
        string weaponName)
    {
        var blastPower = Mathf.Pow(blastYield, 0.3333f);
        var radius = blastPower * 20f;
        var transform = Datum.origin;
        if (PlayerSettings.debugVis)
        {
            transform = UnitRegistry.TryGetNearestUnit(blastPosition.ToGlobalPosition(), out var nearestUnit, 100f)
                ? nearestUnit.transform
                : Datum.origin;
            var gameObject = NetworkSceneSingleton<Spawner>.i.SpawnLocal(GameAssets.i.blastRadiusDebug, transform);
            gameObject.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.0f, 1f));
            gameObject.transform.position = blastPosition;
            gameObject.transform.localScale = Vector3.one * blastPower;
            NetworkSceneSingleton<Spawner>.i.DestroyLocal(gameObject, 10f);
        }

        DamageEffects.hitColliders ??= new Collider[512 /*0x0200*/];
        var num = Physics.OverlapSphereNonAlloc(blastPosition, radius, DamageEffects.hitColliders);
        for (var index = 0; index < num; ++index)
        {
            var hitCollider = DamageEffects.hitColliders[index];
            if (!hitCollider.gameObject.TryGetComponent<IDamageable>(out _)) continue;
            var vector3 = hitCollider.transform.position - blastPosition;
            FragTrace(blastPosition, vector3.normalized * radius, blastYield, blastPower, hitCollider, dealerID,
                transform, weaponName);
        }
    }
}
