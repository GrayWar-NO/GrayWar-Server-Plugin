extern alias NuclearOption;
using Mirage;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using UnityEngine;
// ReSharper disable ConvertToExtensionBlock

namespace GW_server_plugin.Patches.KillsLogging;



/// <summary>
/// New implementations for RPC messages
/// </summary>
public static class RPCMessagesImplementations
{
    /// <summary>
    /// Kill message RPC with weapon names
    /// </summary>
    /// <param name="mgr"></param>
    /// <param name="killerID"></param>
    /// <param name="killedID"></param>
    /// <param name="killedType"></param>
    /// <param name="weaponName"></param>
    public static void RpcKillMessage(
        this MessageManager mgr,
        PersistentID killerID,
        PersistentID killedID,
        KillType killedType,
        string weaponName)
    {
        if (ClientRpcSender.ShouldInvokeLocally(mgr, RpcTarget.Observers, null, false))
            mgr.UserCode_RpcKillMessage_635947223(killerID, killedID, killedType, weaponName);
        var writer = NetworkWriterPool.GetWriter();
        NuclearOption::Mirage.GeneratedNetworkCode._Write_PersistentID(writer, killerID);
        NuclearOption::Mirage.GeneratedNetworkCode._Write_PersistentID(writer, killedID);
        NuclearOption::Mirage.GeneratedNetworkCode._Write_KillType(writer, killedType);
        ClientRpcSender.Send(mgr, 3, writer, Channel.Reliable, false);
        writer.Release();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mgr"></param>
    /// <param name="killerID"></param>
    /// <param name="killedID"></param>
    /// <param name="killedType"></param>
    /// <param name="weaponName"></param>
    private static void UserCode_RpcKillMessage_635947223(
        this MessageManager mgr,
        PersistentID killerID,
        PersistentID killedID,
        KillType killedType,
        string weaponName)
    {
        if (!UnitRegistry.TryGetPersistentUnit(killedID, out var persistentUnit1))
            return;
        var persistentUnit3 = UnitRegistry.TryGetPersistentUnit(killerID, out var persistentUnit2);
        if (!MessageManager.KillFeedFilter(killedType, persistentUnit2, persistentUnit1))
            return;
        var color1 = MessageManager.ColorFromFaction(persistentUnit1.GetHQ());
        var verb = killedType.GetVerb(persistentUnit3);
        if (!persistentUnit3)
        {
            var message = $"{persistentUnit1.unitName.AddColor(color1)} {verb}";
            SceneSingleton<GameplayUI>.i.KillFeed(message);
        }
        else
        {
            var color2 = MessageManager.ColorFromFaction(persistentUnit2.GetHQ());
            var message = $"{persistentUnit2.unitName.AddColor(color2)} {verb} {persistentUnit1.unitName.AddColor(color1)} with {weaponName}";
            SceneSingleton<GameplayUI>.i.KillFeed(message);
        }
    }
    
}
