using BepInEx.Configuration;

namespace GW_server_plugin.Features;

/// <summary>
///     Configuration class for the plugin
/// </summary>
public class PluginConfig
{
    internal const string GeneralSetction = "General";
    internal const string IPCSection = "IPCSection";

    internal static ConfigEntry<string>? BannedPlayers;
    internal const string DefaultBannedPlayers = "";

    internal static ConfigEntry<int>? IpcPort;
    internal const int DefaultIpcPort = 10042;

    internal static ConfigEntry<string>? IpcHost;
    internal const string DefaultIpcHost = "127.0.0.1";

    internal static ConfigEntry<int>? IpcRetryDelayMs;
    internal const int DefaultIpcRetryDelayMs = 5000;

    internal static void InitSettings(ConfigFile config)
    {
        GwServerPlugin.Logger?.LogDebug("Loading Settings...");
        
        BannedPlayers = config.Bind(GeneralSetction, "BannedPlayers", DefaultBannedPlayers);
        GwServerPlugin.Logger?.LogDebug($"BannedPlayers: {BannedPlayers}");
        
        IpcPort = config.Bind(IPCSection, "Communication Port", DefaultIpcPort);
        GwServerPlugin.Logger?.LogDebug($"IpcPort: {IpcPort}");
        
        IpcHost = config.Bind(IPCSection, "Communication Host", DefaultIpcHost);
        GwServerPlugin.Logger?.LogDebug($"IpcHost: {IpcHost}");
        
        IpcRetryDelayMs = config.Bind(IPCSection, "Communication Retry Delay", DefaultIpcRetryDelayMs);
        GwServerPlugin.Logger?.LogDebug("Loading Settings...");
    }
    
    
    
    
}