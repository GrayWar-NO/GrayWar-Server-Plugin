using System;
using System.Linq;
using Com.Graywar.NoServerManager.Proto;
using Google.Protobuf.WellKnownTypes;
using HarmonyLib;
using NuclearOption.Networking;
using Steamworks;

namespace GW_server_plugin.Patches;

/// <summary>
///     Adds internal logging to votekicks and skips the kick entirely if it is a member of staff.
/// </summary>
[HarmonyPatch(typeof(VoteKickManager))]
public class VoteKickLogger
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(VoteKickManager.ExecuteKick))]
    // ReSharper disable once InconsistentNaming
    internal static bool ExecuteKickPrefix(VoteKickManager __instance, ref CSteamID id, string playerName)
    {
        var steamId = id.m_SteamID.ToString();

        var staffIds = PluginConfig.AdminsList
            .Concat(PluginConfig.ModeratorsList)
            .Concat(PluginConfig.Owner?.Value is { } owner ? [owner] : []);

        if (staffIds.Contains(steamId))
            return false;

        var log = new KickLog
        {
            Reason = "Votekicked",
            SteamID = id.m_SteamID,
            Time = DateTime.UtcNow.ToTimestamp()
        };
        GwServerPlugin.GrpcMgr.Client?.SendKickAsync(log);
        return true;
    }
}