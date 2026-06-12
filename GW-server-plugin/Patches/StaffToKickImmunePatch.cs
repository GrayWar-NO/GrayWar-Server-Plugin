using System.Linq;
using GW_server_plugin.Helpers;
using HarmonyLib;
using NuclearOption.DedicatedServer;
using Steamworks;

namespace GW_server_plugin.Patches;

/// <summary>
///     Adds plugin admins to the builtin DedicatedServerConfig's ErrorKickImmune list.
/// </summary>
[HarmonyPatch(typeof(DedicatedServerManager))]
public class StaffToKickImmunePatch
{
        [HarmonyPatch(nameof(DedicatedServerManager.LoadAllowBanList))]
        [HarmonyPostfix]
        internal static void LoadBanListPostfix()
        {
                var staffList = PluginConfig.ModeratorsList.Union(PluginConfig.AdminsList);
                var steamIds = staffList as string[] ?? staffList.ToArray();
                steamIds.AddItem(PluginConfig.Owner!.Value);
                foreach (var steamIDStr in steamIds)
                {
                        if (ulong.TryParse(steamIDStr, out var steamID))
                        {
                                Globals.NetworkManagerNuclearOptionInstance.Authenticator.ErrorKickImmuneList.Add(
                                        new CSteamID(steamID), "");
                        }
                }
                
        }
}