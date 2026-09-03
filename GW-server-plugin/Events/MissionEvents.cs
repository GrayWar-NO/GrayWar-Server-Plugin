using System;
using Com.Graywar.NoServerManager.Proto;
using Google.Protobuf.WellKnownTypes;
using NuclearOption.SavedMission;

namespace GW_server_plugin.Events;

/// <summary>
///     Mission-related events
/// </summary>
public static class MissionEvents
{
    /// <summary>
    ///     Event handler for when a mission starts.
    /// </summary>
    public static event Action<Mission> MissionLoaded = _ => {};

    /// <summary>
    ///     Event handler for when a mission starts.
    /// </summary>
    public static event Action<FactionHQ> MissionEnded = winner =>
    {
        var packet = new MissionStatus
        {
            Ended = true,
            WinnerName = winner.faction.factionName,
            Time = DateTime.UtcNow.ToTimestamp()
        };
        GwServerPlugin.GrpcMgr.Client?.SendMissionChange(packet);
    };

    internal static void OnMissionLoad(Mission e)
    {
        MissionLoaded.Invoke(e);
    }

    internal static void OnMissionEnd(FactionHQ winner)
    {
        MissionEnded.Invoke(winner);
    }
}