using GW_server_plugin.Enums;
using GW_server_plugin.Features.IPC.Packets;
using HarmonyLib;
using NuclearOption.Networking;
using Steamworks;

namespace GW_server_plugin.Patches;

/// <summary>
///     Adds internal logging to votekicks
/// </summary>
[HarmonyPatch(typeof(VoteKickManager))]
public class VoteKickLogger
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(VoteKickManager.ExecuteKick))]
    // ReSharper disable once InconsistentNaming
    internal static void ExecuteKickPostfix(VoteKickManager __instance,ref CSteamID target, string _)
    {
        GwServerPlugin.LoggingOutBox.Add(
            new LogEntryPacket
            {
                Channel = LogChannel.Kick,
                LogText = $"1:{target.m_SteamID.ToString()}:Votekicked",
            });
    }
}