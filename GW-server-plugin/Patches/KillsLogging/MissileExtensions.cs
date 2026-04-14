using NuclearOption.Networking;
using UnityEngine;

namespace GW_server_plugin.Patches.KillsLogging;

/// <summary>
/// Various extension functions for missile class and it's subclasses
/// </summary>
public static class MissileExtensions
{
    /// <summary>
    /// Extension function for HasShockwaveReached
    /// </summary>
    /// <param name="influencedObject"></param>
    /// <param name="blastOrigin"></param>
    /// <param name="blastPropagation"></param>
    /// <param name="overpressure"></param>
    /// <param name="blastYield"></param>
    /// <param name="blastPower"></param>
    /// <param name="ownerID"></param>
    /// <param name="weaponName"></param>
    /// <returns></returns>
    public static bool HasShockwaveReached(
        this Shockwave.InfluencedObject influencedObject,
        Vector3 blastOrigin,
        float blastPropagation,
        float overpressure,
        float blastYield,
        float blastPower,
        PersistentID ownerID,
        string weaponName)
    {
        if (influencedObject.collider == null)
            return true;
        blastPropagation += influencedObject.averageRadius;
        if (Vector3.SqrMagnitude(influencedObject.collider.bounds.center - blastOrigin) > (double) blastPropagation * blastPropagation)
            return false;
        var sphereArea = 3.1415927f * influencedObject.averageRadius * influencedObject.averageRadius;
        var mass = influencedObject.rb != null ? influencedObject.rb.mass : 0.0f;
        var blastDamage = Mathf.Clamp(Mathf.Max(blastPower * blastPower, blastPropagation * blastPropagation) / sphereArea, 0.0f, 10f);
        if (influencedObject.damageable != null)
        {
            var armorProperties = influencedObject.damageable.GetArmorProperties();
            blastDamage /= (float) (1.0 + armorProperties.blastArmor * 0.019999999552965164);
            var unit = influencedObject.damageable.GetUnit();
            mass = influencedObject.damageable.GetMass();
            if (unit is Building building)
                building.RegisterRecentExplosion(blastOrigin.ToGlobalPosition(), blastYield);
            if (NetworkManagerNuclearOption.i.Server.Active)
                influencedObject.damageable.TakeDamage(0.0f, overpressure, Mathf.Clamp01(blastDamage), 0.0f, 0.0f, ownerID, weaponName);
        }

        if (influencedObject.rb == null || !(mass > 0.0)) return true;
        var forceMagnitude = Mathf.Min((float) ((double) sphereArea * Mathf.Clamp01(blastDamage) * overpressure * 25.0), blastYield * 200f, 60f * mass);
        influencedObject.rb.AddForceAtPosition((influencedObject.collider.bounds.center - blastOrigin).normalized * forceMagnitude, influencedObject.collider.bounds.center, ForceMode.Impulse);
        return true;
    }
    
    /// <summary>
    /// Missile.Warhead.Detonate override
    /// </summary>
    /// <param name="warhead"></param>
    /// <param name="rb"></param>
    /// <param name="ownerID"></param>
    /// <param name="position"></param>
    /// <param name="normal"></param>
    /// <param name="armed"></param>
    /// <param name="blastYield"></param>
    /// <param name="hitArmor"></param>
    /// <param name="hitTerrain"></param>
    /// <param name="weaponName"></param>
    public static void Detonate(
        this Missile.Warhead warhead,
        Rigidbody rb,
        PersistentID ownerID,
        Vector3 position,
        Vector3 normal,
        bool armed,
        float blastYield,
        bool hitArmor,
        bool hitTerrain,
        string weaponName)
    {
      if (warhead.detonated)
        return;
      warhead.detonated = true;
      if (!armed)
      {
        if (warhead.fizzleEffect == null)
          return;
        Object.Instantiate(warhead.fizzleEffect, Datum.origin).transform.SetPositionAndRotation(rb.position, FastMath.LookRotation(rb.velocity));
      }
      else
      {
        var blastPower = Mathf.Pow(blastYield, 0.3333f) * 2f;
        GameObject? detonationEffect = null;
        var isUnderTheSea = position.y < Datum.LocalSeaY + 0.10000000149011612 ? 1 : 0;
        var missileAtSeaSurface = new Vector3(position.x, Datum.LocalSeaY, position.z);
        if (isUnderTheSea != 0)
        {
          detonationEffect = Object.Instantiate(warhead.underwaterEffect, Datum.origin);
          detonationEffect.transform.SetPositionAndRotation(missileAtSeaSurface, Quaternion.identity);
        }
        else
        {
          if (hitTerrain)
          {
            detonationEffect = Object.Instantiate(warhead.terrainEffect, Datum.origin);
            detonationEffect.transform.SetPositionAndRotation(position, Quaternion.LookRotation(normal));
          }
          if (hitArmor)
          {
            detonationEffect = Object.Instantiate(warhead.armorEffect, Datum.origin);
            detonationEffect.transform.SetPositionAndRotation(position, Quaternion.LookRotation(normal));
          }

          var hitWaterSurface = hitTerrain || Physics.Linecast(position, position - Vector3.up * blastPower, out var hitInfo, 64 /*0x40*/) && hitInfo.point.y > (double) Datum.LocalSeaY;
          if (warhead.waterSurfaceEffect != null && !hitWaterSurface && position.y < Datum.LocalSeaY + (double) blastPower && position.y > Datum.LocalSeaY + 1.0)
          {
            var gameObject2 = Object.Instantiate(warhead.waterSurfaceEffect, Datum.origin);
            gameObject2.transform.SetPositionAndRotation(missileAtSeaSurface, Quaternion.identity);
            Object.Destroy(gameObject2, 30f);
          }
        }
        if (detonationEffect == null)
        {
          detonationEffect = Object.Instantiate(warhead.airEffect, Datum.origin);
          detonationEffect.transform.SetPositionAndRotation(position, FastMath.LookRotation(rb.velocity));
        }
        if (blastYield > 200.0)
        {
          var shockwave = detonationEffect.GetComponentInChildren<Shockwave>();
          if (!(shockwave != null))
            return;
          var swWeaponTypeStorage = GwServerPlugin.ShockwaveWeaponStorage.Get(shockwave);
          swWeaponTypeStorage.WeaponName = weaponName;
          shockwave.SetOwner(ownerID, blastYield * 1E-06f);
        }
        else
          Object.Destroy(detonationEffect, 30f);
      }
    }
}
