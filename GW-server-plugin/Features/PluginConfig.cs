using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace GW_server_plugin.Features;

/// <summary>
///     Configuration class for the plugin
/// </summary>
public static class PluginConfig
{
    internal const string GeneralSection = "General";
    internal const string IpcSection = "IPCSection";

    internal static ConfigEntry<string>? BannedPlayers;
    internal const string DefaultBannedPlayers = "";

    internal static ConfigEntry<int>? IpcPort;
    internal const int DefaultIpcPort = 10042;

    internal static ConfigEntry<string>? IpcHost;
    internal const string DefaultIpcHost = "127.0.0.1";

    internal static ConfigEntry<int>? IpcRetryDelayMs;
    internal const int DefaultIpcRetryDelayMs = 5000;

    internal static ConfigEntry<string>? IpcCommandPermissionLevel;
    internal const string DefaultIpcCommandPermissionLevel = "admin";
    

    internal static ConfigEntry<string>? Moderators;
    internal const string DefaultModerators = "";
    
    internal static ConfigEntry<string>? Admins;
    internal const string DefaultAdmins = "";
    
    internal static ConfigEntry<string>? Owner;
    internal const string DefaultOwner = "";

    internal static List<string> ModeratorsList => Moderators!.Value.Split(';').Where(m => !string.IsNullOrWhiteSpace(m)).ToList();
    
    internal static List<string> AdminsList => Admins!.Value.Split(';').Where(a => !string.IsNullOrWhiteSpace(a)).ToList();


    internal static void InitSettings(ConfigFile config)
    {
        GwServerPlugin.Logger?.LogDebug("Loading Settings...");
        
        BannedPlayers = config.Bind(GeneralSection, "BannedPlayers", DefaultBannedPlayers);
        GwServerPlugin.Logger?.LogDebug($"BannedPlayers: {BannedPlayers}");
        
        Moderators = config.Bind(GeneralSection, "Moderators", DefaultModerators, "A list of moderators who have access to moderator commands. Separate steam IDs with a semicolon.");
        GwServerPlugin.Logger?.LogDebug($"Moderators: {Moderators.Value}");
        
        Admins = config.Bind(GeneralSection, "Admins", DefaultAdmins, "A list of admins who have access to admin commands. Separate steam IDs with a semicolon.");
        GwServerPlugin.Logger?.LogDebug($"Admins: {Admins.Value}");
        
        Owner = config.Bind(GeneralSection, "Owner", DefaultOwner, "The Steam ID of the server owner. This player has access to all commands, and cannot be removed from the admin list.");
        GwServerPlugin.Logger?.LogDebug($"Owner: {Owner.Value}");

        
        IpcPort = config.Bind(IpcSection, "Communication Port", DefaultIpcPort);
        GwServerPlugin.Logger?.LogDebug($"IpcPort: {IpcPort}");
        
        IpcHost = config.Bind(IpcSection, "Communication Host", DefaultIpcHost);
        GwServerPlugin.Logger?.LogDebug($"IpcHost: {IpcHost}");
        
        IpcRetryDelayMs = config.Bind(IpcSection, "Communication Retry Delay", DefaultIpcRetryDelayMs);
        GwServerPlugin.Logger?.LogDebug($"Retry delay: {IpcRetryDelayMs}");
        
        IpcCommandPermissionLevel = config.Bind(IpcSection, "Communication Permission Level", DefaultIpcCommandPermissionLevel);
        GwServerPlugin.Logger?.LogDebug($"Communication Permission level: {IpcCommandPermissionLevel}");
        
        GwServerPlugin.Logger?.LogDebug($"Loaded settings.");
    }
    
    
    
    
}