using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Configuration;
using GW_server_plugin.Helpers;

namespace GW_server_plugin.Features.CommandUtils;

/// <summary>
///     Manages the timely restarting of this server for minimal impact.
/// </summary>
public static class RestartService
{
    private static ConfigEntry<bool> _enableForceRestart = null!;
    private static ConfigEntry<bool> _enableNoPlayersRestart = null!;
    private static ConfigEntry<uint> _noPlayersRestartTimeout = null!;
    private static ConfigEntry<uint> _forceRestartMaxInterval = null!;
    
    internal static bool AwaitingRestart;
    
    private static CancellationTokenSource? _restartCts;
    
    /// <summary>
    ///     Initializes the config variables for the restartservice.
    /// </summary>
    /// <param name="config"></param>
    public static void Initialize(ConfigFile config)
    {
        _enableForceRestart = config.Bind("RestartService", "enableForceRestart", true,
            "Enable force restart after a set duration");
        _forceRestartMaxInterval = config.Bind("RestartService", "forceRestartMaxInterval", 24u,
            "Maximum allowed restart interval (in hours)");
        _enableNoPlayersRestart = config.Bind("RestartService", "enableNoPlayersRestart", true,
            "Enables restarting when no players are on the server.");
        _noPlayersRestartTimeout = config.Bind("RestartService", "noPlayersRestartTimeout", 60u,
            "How long should the server wait to restart after the last player leaves (in seconds)");
    }
    
    /// <summary>
    ///     Checks player count. If it's 0, start the restart timer.
    /// </summary>
    public static void CheckIfNoPlayers()
    {
        if (!_enableNoPlayersRestart.Value || PlayerUtils.GetPlayerCount() != 0) return;
        // Only start the timer if one isn't already running
        if (_restartCts != null) return;
        _restartCts = new CancellationTokenSource();
        _ = ScheduleRestartAsync(_restartCts.Token);
    }
    
    /// <summary>
    ///     Cancel any pending restart.
    /// </summary>
    public static void CancelRestart()
    {
        // Player joined — cancel any pending restart
        if (_restartCts == null) return;
        GwServerPlugin.Logger.LogInfo("A Player joined. Restart canceled");
        _restartCts.Cancel();
        _restartCts = null;
    }
    
    private static async Task ScheduleRestartAsync(CancellationToken ct)
    {
        try
        {
            GwServerPlugin.Logger.LogInfo(
                $"No players. Waiting {_noPlayersRestartTimeout.Value} seconds to restart...");
            await Task.Delay(TimeSpan.FromSeconds(_noPlayersRestartTimeout.Value), ct);
            
            // Re-check after delay
            if (PlayerUtils.GetPlayerCount() == 0)
            {
                GwServerPlugin.Logger.LogInfo("RESTARTING SERVER...");
                Restart();
            }
        }
        catch (TaskCanceledException)
        {
            // Players rejoined before restart — do nothing
        }
        catch (Exception e)
        {
            GwServerPlugin.Logger.LogError(e);
        }
        finally
        {
            _restartCts = null;
        }
    }
    
    /// <summary>
    ///     Restarts the server via the docker socket.
    /// </summary>
    /// <returns></returns>
    public static bool Restart()
    {
        try
        {
            GwServerPlugin.Logger.LogInfo("AUTO-RESTARTING SERVER - Restart()");
            using HttpClient client = new();
            var hostname = Environment.MachineName;
            var dockerHost = Environment.GetEnvironmentVariable("DOCKER_HOST");
            var dockerURL = $"{dockerHost}/containers/{hostname}/restart";
            GwServerPlugin.Logger.LogInfo(dockerURL);
            
            var process = new Process();
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
        finally
        {
            AwaitingRestart = false;
        }
    }
    
    /// <summary>
    ///     Forces restart if the server ever is awake for more than 24 hours.
    /// </summary>
    public static void AutoRestart()
    {
        if (!_enableForceRestart.Value) return;
        if (DateTime.Now.Subtract(GwServerPlugin.ServerStartTime).Hours < _forceRestartMaxInterval.Value) return;
        GwServerPlugin.Logger.LogInfo("AUTO-RESTARTING SERVER");
        AwaitingRestart = true;
    }
}