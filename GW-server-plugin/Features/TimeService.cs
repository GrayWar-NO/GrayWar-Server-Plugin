using GW_server_plugin.Events;
using UnityEngine;

namespace GW_server_plugin.Features;

/// <summary>
///     Time service for the plugin. Handles triggering of time-based events.
/// </summary>
public class TimeService : MonoBehaviour
{
    public static int _lastWholeSecond = 1;

    /// <summary>
    ///     The singleton instance of the time service.
    /// </summary>
    public static TimeService? Instance { get; private set; }

    /// <summary>
    ///     Initializes the time service.
    /// </summary>
    public static void Initialize()
    {
        if (Instance != null) return;

        var go = new GameObject("TimeService")
        {
            hideFlags = HideFlags.DontSave
        };
        Instance = go.AddComponent<TimeService>();
    }

    /// <summary>
    ///     Destroys the time service.
    /// </summary>
    public static void Destroy()
    {
        if (Instance == null) return;
        Destroy(Instance.gameObject);
        Instance = null;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);
        GwServerPlugin.Logger.LogInfo("Time Service started.");
    }
    
    private void Update()
    {
        int whole = Mathf.FloorToInt(Time.unscaledTime);
        if (whole <= _lastWholeSecond) return;

        // Fire once per missed second (handles frame hiccups)
        for (int s = _lastWholeSecond + 1; s <= whole; s++)
            FireForSecond(s);

        _lastWholeSecond = whole;
    }
    
    private static void FireForSecond(int secondsSinceStart)
    {
        TimeEvents.OnEverySecond();

        if (secondsSinceStart % 30 == 0)
            TimeEvents.OnEvery30Seconds();

        if (secondsSinceStart % 60 == 0)
            TimeEvents.OnEveryMinute();

        if (secondsSinceStart % 600 == 0)
            TimeEvents.OnEvery10Minutes();

        if (secondsSinceStart % 1800 == 0)
            TimeEvents.OnEvery30Minutes();

        if (secondsSinceStart % 3600 == 0)
            TimeEvents.OnEveryHour();
    }
}