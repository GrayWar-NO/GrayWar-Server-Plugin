using System;
using System.Threading.Tasks;

namespace GW_server_plugin.Features;

/// <summary>
/// Class that contains scheduled warnings code.
/// </summary>
public static class RestartWarningService
{
    internal static void ScheduleWarnings()
    {
        var now = DateTime.UtcNow;

        // Next restart at 05:00 UTC
        var restartTime = now.Date.AddHours(5);
        
        // If today's restart already passed, target tomorrow
        if (now >= restartTime)
        {
            restartTime = restartTime.AddDays(1);
        }

        ScheduleWarning(restartTime, TimeSpan.FromHours(1), "1 hour");
        ScheduleWarning(restartTime, TimeSpan.FromMinutes(30), "30 minutes");
        ScheduleWarning(restartTime, TimeSpan.FromMinutes(10), "10 minutes");
        ScheduleWarning(restartTime, TimeSpan.FromMinutes(5), "5 minutes");
        ScheduleWarning(restartTime, TimeSpan.FromMinutes(1), "1 minute");
    }

    private static void ScheduleWarning(DateTime restartTime, TimeSpan beforeRestart, string message)
    {
        var warningTime = restartTime - beforeRestart;
        var delay = warningTime - DateTime.UtcNow;

        // Skip if the warning time already passed
        if (delay <= TimeSpan.Zero)
            return;

        _ = WarningTask(delay, message);
    }

    private static async Task WarningTask(TimeSpan delay, string timeString)
    {
        try
        {
            await Task.Delay(delay);

            ChatService.SendChatMessageAsServer(
                $"Daily server restart will occur in {timeString}! This allows our servers to keep running smoothly.");
        }
        catch (Exception ex)
        {
            GwServerPlugin.Logger.LogError($"Warning task failed: {ex}");
        }
    }
}