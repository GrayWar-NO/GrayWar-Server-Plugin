using System.Collections.Generic;
using System.Linq;
using GW_server_plugin.Enums;
using GW_server_plugin.Features.IPC.Packets;
using Newtonsoft.Json;
using NuclearOption.Networking;
using UnityEngine;

namespace GW_server_plugin.Patches.KillsLogging;

/// <summary>
/// Unit extensions for weapon logging.
/// </summary>
public static class WeaponLoggingExtensions
{
    /// <summary>
    /// Records damage on an unit.
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="lastDamagedBy"></param>
    /// <param name="damageAmount"></param>
    /// <param name="weaponName"></param>
    // ReSharper disable once ConvertToExtensionBlock
    public static void RecordDamage(
        this Unit unit,
        PersistentID lastDamagedBy,
        float damageAmount,
        string weaponName)
    {
        if (unit == null)
        {
            GwServerPlugin.Logger.LogError("Unit is null in recordDamage!");
            return;
        }
        unit.damageCredit ??= new Dictionary<PersistentID, float>();

        unit.damageCredit.TryGetValue(lastDamagedBy, out var originalDamageAmount);
        unit.damageCredit[lastDamagedBy] = originalDamageAmount + damageAmount;

        var state = GwServerPlugin.WeaponStorage.Get(unit);
        var weaponCredit = state.WeaponCredit;

        if (!weaponCredit.TryGetValue(lastDamagedBy, out var existingDamageCredit))
        {
            existingDamageCredit = new Dictionary<string, float>
            {
                [weaponName] = damageAmount
            };
        }
        else
        {
            existingDamageCredit[weaponName] =
                existingDamageCredit.TryGetValue(weaponName, out var current)
                    ? current + damageAmount
                    : damageAmount;
        }

        weaponCredit[lastDamagedBy] = existingDamageCredit;
    }

    /// <summary>
    /// Unit.ReportKilled implementation that takes the weapon logging into account.
    /// </summary>
    public static void ReportKilled(this Unit unit)
    {
        var killerID = PersistentID.None;
        if (!UnitRegistry.TryGetPersistentUnit(unit.persistentID, out var killedUnit))
            return;
        // ReSharper disable once InconsistentNaming
        var killedHQ = killedUnit.GetHQ();
        var killerDamage = 0.0f;
        var totalReceivedDamage = 0.0f;
        var state = GwServerPlugin.WeaponStorage.Get(unit);
        var weaponCredit = state.WeaponCredit;
        if (unit.damageCredit != null)

        {
            totalReceivedDamage += unit.damageCredit.Sum(keyValuePair => keyValuePair.Value);

            var damageDealerPlayers = new Dictionary<Player, float>();
            foreach (var receivedDamage in unit.damageCredit)
            {
                if (!UnitRegistry.TryGetPersistentUnit(receivedDamage.Key, out var damageDealerUnit)) continue;
                var dealtDamageProportion = receivedDamage.Value / totalReceivedDamage;
                if (dealtDamageProportion < 0.009999999776482582)
                    continue; // process only if dealt damage is significant (over 1%)
                if (receivedDamage.Value >= (double)killerDamage)
                {
                    // Update max damage dealer (killer)
                    killerDamage = receivedDamage.Value;
                    killerID = receivedDamage.Key;
                }

                // ReSharper disable once InconsistentNaming
                var damageDealerHQ = damageDealerUnit.GetHQ();
                if (killedHQ == damageDealerHQ || killedHQ == null) continue;
                var score = Mathf.Sqrt(killedUnit.definition.value) * dealtDamageProportion;
                var reward = score * damageDealerHQ.killReward;
                damageDealerHQ.AddScore(score);
                damageDealerHQ.AddFunds(reward * damageDealerHQ.playerTaxRate);
                if (damageDealerUnit.player == null) continue;
                // add player to dict of damage dealer players
                if (!damageDealerPlayers.ContainsKey(damageDealerUnit.player))
                    damageDealerPlayers.Add(damageDealerUnit.player, 0.0f);
                damageDealerPlayers[damageDealerUnit.player] += dealtDamageProportion;
            }

            foreach (var damageDealer in damageDealerPlayers)
                damageDealer.Key.HQ.ReportKillAction(damageDealer.Key, unit, damageDealer.Value);
        }

        ulong? killedSteamID;
        Aircraft? killedAircraft;
        if (killedUnit.unit is Aircraft killedAircraftUnit)
        {
            killedAircraft = killedAircraftUnit;
            killedSteamID = killedAircraft.Player?.SteamID;
        }
        else
        {
            killedAircraft = null;
            killedSteamID = null;
        }

        var killerIsUnit = UnitRegistry.TryGetUnit(killerID, out var killerUnit);
        ulong? killerSteamID;
        Aircraft? killerAircraft;
        if (killerUnit is Aircraft killerAircraftUnit)
        {
            killerAircraft = killerAircraftUnit;
            killerSteamID = killerAircraft.Player?.SteamID;
        }
        else
        {
            killerAircraft = null;
            killerSteamID = null;
        }

        KeyValuePair<string, float>? killerWeapon;
        string killerWeaponName;
        if (!weaponCredit.TryGetValue(killerID, out var killerAircraftWeapons))
        {
            killerWeaponName = "UNKNOWN";
        }
        else if (killerAircraftWeapons is null)
        {
            GwServerPlugin.Logger.LogError(
                "This should not happen. Something killed something else without recording any dealt damage");
            killerWeaponName = "UNKNOWN";
        }
        else
        {
            killerWeapon = killerAircraftWeapons.FirstOrDefault();
            foreach (var kvp in killerAircraftWeapons.Where(kvp => kvp.Value > killerWeapon.Value.Value))
            {
                killerWeapon = kvp;
            }
            killerWeaponName = killerWeapon?.Key ?? "UNKNOWN";
        }

        GwServerPlugin.Logger.LogDebug($"An {unit.unitName} was killed with weapon {killerWeaponName}");

        var logPacket = new LogEntryPacket
        {
            LogText =
                $"{killerSteamID?.ToString() ?? ""}:{killerUnit?.unitName ?? ""}:{killerWeaponName}:{killedSteamID?.ToString() ?? ""}:{killedUnit.unitName}"
        };

        if (killedAircraft is not null &&
            killedAircraft.Player != null &&
            totalReceivedDamage > 1.0 && killerIsUnit &&
            killerUnit!.NetworkHQ == killedAircraft.NetworkHQ &&
            killerAircraft is not null &&
            killerAircraft.Player != null)
        {
            // if TEAMKILL
            logPacket.Channel = LogChannel.Teamkill;
            var amount = killedAircraft.definition.value + killedAircraft.weaponManager.GetCurrentValue(true);
            killerAircraft.Player.AddScore(-Mathf.Sqrt(killedAircraft.definition.value));
            killerAircraft.Player.AddAllocation(-amount);
            killedAircraft.Player.AddAllocation(amount);
            GwServerPlugin.OnTeamkill(killerAircraft.Player, killedAircraft.Player, killerWeaponName);
        }
        else logPacket.Channel = LogChannel.Kill;
        
        if (killerSteamID != null || killedSteamID != null)
            GwServerPlugin.SocketOutBox.Add(JsonConvert.SerializeObject(logPacket));

        // ReSharper disable once RedundantCast
        var killedType = unit switch
        {
            Missile _ => KillType.Missile,
            Building _ => KillType.Building,
            Aircraft _ => KillType.Aircraft,
            Ship _ => KillType.Ship,
            _ => KillType.Vehicle
        };

        if (NetworkSceneSingleton<MessageManager>.i == null)
            return;
        NetworkSceneSingleton<MessageManager>.i.RpcKillMessage(killerID, unit.persistentID, killedType,
            killerWeaponName);
    }
}
