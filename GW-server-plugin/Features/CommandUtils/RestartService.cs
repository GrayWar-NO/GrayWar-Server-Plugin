using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GW_server_plugin.Helpers;

namespace GW_server_plugin.Features.CommandUtils;

public static class RestartService
{
    private static CancellationTokenSource? _restartCts;
    
    /// <summary>
    /// Checks player count. If it's 0, start the restart timer.
    /// </summary>
    public static void CheckIfNoPlayers()
    {
        if (PlayerUtils.GetPlayerCount() == 0)
        {
            // Only start the timer if one isn't already running
            if (_restartCts == null)
            {
                _restartCts = new CancellationTokenSource();
                _ = ScheduleRestartAsync(_restartCts.Token);
            }
        }
    }

    public static void CancelRestart()
    {
        // Player joined — cancel any pending restart
        if (_restartCts != null)
        {
            GwServerPlugin.Logger?.LogInfo($"A Player joined. Restart canceled");
            _restartCts?.Cancel();
            _restartCts = null;
        }
    }

    private static async Task ScheduleRestartAsync(CancellationToken ct)
    {
        try
        {
            GwServerPlugin.Logger?.LogInfo($"No players. Waiting 60 seconds to restart...");
            await Task.Delay(60000, ct);

            // Re-check after delay
            if (PlayerUtils.GetPlayerCount() == 0)
            {
                GwServerPlugin.Logger?.LogInfo("RESTARTING SERVER...");
                Restart();
            }
        }
        catch (TaskCanceledException)
        {
            // Players rejoined before restart — do nothing
        }
        catch (Exception e)
        {
            GwServerPlugin.Logger?.LogError(e);
        }
        finally
        {
            _restartCts = null;
        }
    }
    public static bool Restart()
    {
        
        try
        {
            GwServerPlugin.Logger.LogInfo("AUTORESTARTING SERVER - Restart()");
            using HttpClient client = new();
            var hostname = System.Environment.MachineName;
            var dockerHost = System.Environment.GetEnvironmentVariable("DOCKER_HOST");
            var dockerURL = $"{dockerHost}/containers/{hostname}/restart";
            GwServerPlugin.Logger.LogInfo(dockerURL);
        
            Process process = new Process();
            process.StartInfo.FileName = "curl";
            process.StartInfo.Arguments = $"-X POST {dockerURL}";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            return true;
        }
        catch (Exception e)
        {
            GwServerPlugin.Logger.LogError(e);
            return false;
        }
    }

    public static void AutoRestart()
    {
        if (DateTime.Now.Subtract(GwServerPlugin.ServerStartTime).Hours >= 24)
        {
            GwServerPlugin.Logger.LogInfo("AUTORESTARTING SERVER");
            Restart();
        }
    }
}