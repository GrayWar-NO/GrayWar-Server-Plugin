using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using GW_server_plugin.Enums;

namespace GW_server_plugin;

/// <summary>
///     Configuration class for the plugin
/// </summary>
public static class PluginConfig
{
    internal const string GeneralSection = "General";
    internal const string IpcSection = "IPCSection";
    
    internal static ConfigEntry<string>? CommandPrefix;
    internal const string DefaultCommandPrefix = "/";


    internal static ConfigEntry<bool>? EnableWeatherRandomizer;
    internal const bool DefaultWeatherRandomizer = true;    

    internal static ConfigEntry<int>? IpcPort;
    internal const int DefaultIpcPort = 10042;

    internal static ConfigEntry<string>? IpcHost;
    internal const string DefaultIpcHost = "127.0.0.1";
    
    internal static ConfigEntry<string>? IpcCommandPermissionLevel;
    internal const string DefaultIpcCommandPermissionLevel = "admin";

    internal static ConfigEntry<bool>? IpcEnable;
    internal const bool DefaultIpcEnable = true;

    internal static ConfigEntry<bool>? UseStaffPrefix;
    internal const bool DefaultUseStaffPrefix = true;

    internal static ConfigEntry<string>? StaffPrefix;
    internal const string DefaultStaffPrefix = "<color=#FFD700>[Staff]</color>";

    internal static ConfigEntry<string>? ServerBroadcastName;
    internal const string DefaultServerBroadcastName = "<color=#99182e>[GrayWar]</color>";

    internal static ConfigEntry<bool>? EnableTeamDamageAutoWarning;
    internal const bool DefaultEnableTeamDamageAutoWarning = true;
    

    internal static ConfigEntry<string>? Moderators;
    internal const string DefaultModerators = "";
    
    internal static ConfigEntry<string>? Admins;
    internal const string DefaultAdmins = "";
    
    internal static ConfigEntry<string>? Owner;
    internal const string DefaultOwner = "";

    internal static List<string> ModeratorsList => Moderators!.Value.Split(';').Where(m => !string.IsNullOrWhiteSpace(m)).ToList();
    
    internal static List<string> AdminsList => Admins!.Value.Split(';').Where(a => !string.IsNullOrWhiteSpace(a)).ToList();

    internal static char CommandPrefixChar => CommandPrefix!.Value[0];

    internal static void InitSettings(ConfigFile config)
    {
        GwServerPlugin.Logger.LogDebug("Loading Settings...");

        CommandPrefix = config.Bind(GeneralSection, "CommandPrefix", DefaultCommandPrefix, "What to use as the command prefix (the character at the start of a command).");
        GwServerPlugin.Logger.LogDebug($"CommandPrefix: {CommandPrefix.Value}");
        
        Moderators = config.Bind(GeneralSection, "Moderators", DefaultModerators, "A list of moderators who have access to moderator commands. Separate steam IDs with a semicolon.");
        GwServerPlugin.Logger.LogDebug($"Moderators: {Moderators.Value}");
        
        Admins = config.Bind(GeneralSection, "Admins", DefaultAdmins, "A list of admins who have access to admin commands. Separate steam IDs with a semicolon.");
        GwServerPlugin.Logger.LogDebug($"Admins: {Admins.Value}");
        
        Owner = config.Bind(GeneralSection, "Owner", DefaultOwner, "The Steam ID of the server owner. This player has access to all commands, and cannot be removed from the admin list.");
        GwServerPlugin.Logger.LogDebug($"Owner: {Owner.Value}");
        
        UseStaffPrefix = config.Bind(GeneralSection, "UseStaffPrefix", DefaultUseStaffPrefix,
            "Whether to use staff prefix or not.");
        GwServerPlugin.Logger.LogDebug($"UseStaffPrefix: {UseStaffPrefix.Value}");

        StaffPrefix = config.Bind(GeneralSection, "StaffPrefix", DefaultStaffPrefix,
            "The prefix added in-front of the usernames of Moderators, Admins and the Owner.");
        GwServerPlugin.Logger.LogDebug($"StaffTag: {StaffPrefix.Value}");

        EnableWeatherRandomizer = config.Bind(GeneralSection, "Enable weather randomizer", DefaultWeatherRandomizer);
        GwServerPlugin.Logger.LogDebug($"Weather randomizer {(EnableWeatherRandomizer.Value ? "enabled" : "disabled")}.");
        
        
        ServerBroadcastName = config.Bind(GeneralSection, "ServerBroadcastName", DefaultServerBroadcastName,
            "The name that appears in the chat when the server broadcasts a message.");
        GwServerPlugin.Logger.LogDebug($"ServerBroadcastName: {ServerBroadcastName}");

        EnableTeamDamageAutoWarning = config.Bind(GeneralSection, "Enable team damage automatic warning",
            DefaultEnableTeamDamageAutoWarning);
        

        
        IpcEnable = config.Bind(IpcSection, "Enable IPC", DefaultIpcEnable);
        GwServerPlugin.Logger.LogDebug($"IpcPort: {IpcEnable}");

        IpcPort = config.Bind(IpcSection, "Communication Port", DefaultIpcPort);
        GwServerPlugin.Logger.LogDebug($"IpcPort: {IpcPort}");
        
        IpcHost = config.Bind(IpcSection, "Communication Host", DefaultIpcHost);
        GwServerPlugin.Logger.LogDebug($"IpcHost: {IpcHost}");
        
        IpcCommandPermissionLevel = config.Bind(IpcSection, "Communication Permission Level", DefaultIpcCommandPermissionLevel);
        GwServerPlugin.Logger.LogDebug($"Communication Permission level: {IpcCommandPermissionLevel}");
        
        GwServerPlugin.Logger.LogDebug("Loaded settings.");
    }
    
    /// <summary>
    ///     Check if the given Steam ID is a moderator.
    /// </summary>
    /// <param name="steamId"> The Steam ID to check. </param>
    /// <returns> Whether the Steam ID is a moderator. </returns>
    public static bool IsModerator(ulong steamId)
    {
        return ModeratorsList.Contains(steamId.ToString());
    }

    /// <summary>
    ///     Check if the given Steam ID is an admin.
    /// </summary>
    /// <param name="steamId"> The Steam ID to check. </param>
    /// <returns> Whether the Steam ID is an admin. </returns>
    public static bool IsAdmin(ulong steamId)
    {
        return AdminsList.Contains(steamId.ToString());
    }

    /// <summary>
    ///     Check if the given Steam ID is the owner.
    /// </summary>
    /// <param name="steamId"> The Steam ID to check. </param>
    /// <returns> Whether the Steam ID is the owner. </returns>
    public static bool IsOwner(ulong steamId)
    {
        return Owner!.Value == steamId.ToString();
    }

    /// <summary>
    /// Removes Admin perms for an user
    /// </summary>
    /// <param name="steamId">User steamID</param>
    public static void RemoveAdmin(ulong steamId)
    {
        var adminsList = AdminsList;
        adminsList.Remove(steamId.ToString());
        Admins!.Value = string.Join(";", adminsList);
    }

    /// <summary>
    /// Removes Moderator perms for an user
    /// </summary>
    /// <param name="steamId">User steamID</param>
    public static void RemoveMod(ulong steamId)
    {
        var modsList = ModeratorsList;
        modsList.Remove(steamId.ToString());
        Moderators!.Value = string.Join(";", modsList);
    }

    /// <summary>
    /// Clears all permissions for an user.
    /// </summary>
    /// <param name="steamId">User steamID</param>
    public static void ClearPermissions(ulong steamId)
    {
        RemoveAdmin(steamId);
        RemoveMod(steamId);
    }

    /// <summary>
    /// Sets an user's permission level.
    /// </summary>
    /// <param name="steamId">User SteamID</param>
    /// <param name="level">Permission level to give</param>
    public static void SetPermissionLevel(ulong steamId, PermissionLevel level)
    {
        ClearPermissions(steamId);
        switch (level)
        {
            case PermissionLevel.Admin:
                var adminsList = AdminsList;
                adminsList.Add(steamId.ToString());
                Admins!.Value = string.Join(";", adminsList);
                break;
            case PermissionLevel.Moderator:
                var modsList = ModeratorsList;
                modsList.Add(steamId.ToString());
                Moderators!.Value = string.Join(";", modsList);
                break;
            case PermissionLevel.Everyone:
            case PermissionLevel.Owner:
            default:
                break;
        }
    }
    
}