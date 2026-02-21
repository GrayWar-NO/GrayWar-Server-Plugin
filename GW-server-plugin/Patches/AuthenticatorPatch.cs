using System.Collections.Generic;
using HarmonyLib;
using NuclearOption.Networking.Authentication;
using Steamworks;

namespace GW_server_plugin.Patches;

[HarmonyPatch(typeof(NetworkAuthenticatorNuclearOption))]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
internal static class AuthenticatorPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(NetworkAuthenticatorNuclearOption.ValidateAuthTicketResponse))]
    private static bool ValidateAuthTicketResponsePrefix(
        ValidateAuthTicketResponse_t param)
    {
#pragma warning disable Harmony003
        GwServerPlugin.FamilySharingBorrowers[param.m_SteamID.m_SteamID] = param.m_OwnerSteamID.m_SteamID;
#pragma warning restore Harmony003
        return true;
    }
}