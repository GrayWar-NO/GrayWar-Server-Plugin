using GW_server_plugin.Helpers;
using Steamworks;

namespace GW_server_plugin.Features;

/// <summary>
/// Service for staff-only player slots.
/// </summary>
public static class StaffSlotService
{
    private static int _nStaffSlots;

    /// <summary>
    /// Adds a slot to the server, that will be reserved for staff members.
    /// </summary>
    public static void AddStaffSlot()
    {
        _nStaffSlots++;
        Globals.NetworkManagerNuclearOptionInstance.Server.PeerConfig.MaxConnections += 1;
        SteamGameServer.SetMaxPlayerCount(Globals.NetworkManagerNuclearOptionInstance.Server.PeerConfig.MaxConnections);
    }

    /// <summary>
    /// Removes a staff-only slot from the server. 
    /// </summary>
    public static bool RemoveStaffSlot()
    {
        if (_nStaffSlots <= 0) return false;
        _nStaffSlots--;
        Globals.NetworkManagerNuclearOptionInstance.Server.PeerConfig.MaxConnections -= 1;
        SteamGameServer.SetMaxPlayerCount(Globals.NetworkManagerNuclearOptionInstance.Server.PeerConfig.MaxConnections);
        return true;
    }

    /// <summary>
    /// Checks if the slot with given index is staff-only.
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public static bool IsSlotStaff(int slot)
    {
        var unusedStaffSlots = _nStaffSlots - PlayerUtils.CountStaff();
        if (unusedStaffSlots <= 0) return false;
        var total = Globals.NetworkManagerNuclearOptionInstance.Server.PeerConfig.MaxConnections;
        var nPublic = total - _nStaffSlots;
        return slot > nPublic;
    }
    
}