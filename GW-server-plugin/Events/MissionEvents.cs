using System;
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

    internal static void OnMissionLoad(Mission e)
    {
        MissionLoaded.Invoke(e);
    }
}