using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Com.Graywar.NoServerManager.Proto;
using Google.Protobuf.WellKnownTypes;
using GW_server_plugin.Events;
using GW_server_plugin.Features;
using GW_server_plugin.Features.CommandUtils;
using GW_server_plugin.Features.Protobuf_IPC;
using GW_server_plugin.Helpers;
using GW_server_plugin.Patches.KillsLogging;
using HarmonyLib;
using JetBrains.Annotations; using NuclearOption.Networking;
using Steamworks;

namespace GW_server_plugin;

/// <summary>
/// Main plugin class for the plugin
/// </summary>
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class GwServerPlugin : BaseUnityPlugin
{
    internal static GwServerPlugin Instance { get; private set; } = null!;
    
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static PlayerIdentificationService PlayerIdentifier { get; private set; } = null!;
    
    internal static MissionVoteService MissionVote { get; private set; } = null!;

    internal static WeatherRandomizer WeatherRandomizer { get; private set; } = null!;

    private static MissionBalanceService MissionBalance { get; set; } = null!;

    internal static WarnService WarnService { get; private set; } = null!;

    /// <summary>
    /// Weapon type storage for weapon kill detection.
    /// </summary>
    public static readonly UnitWeaponLogStorage WeaponStorage = new();

    /// <summary>
    /// Weapon name storage for shockwaves.
    /// </summary>
    public static readonly ShockwaveWeaponTypeStorage ShockwaveWeaponStorage = new();
    
    private static Harmony? Harmony { get; set; }
    private static bool IsPatched { get; set; }
    
    internal static readonly Dictionary<ulong, ulong> FamilySharingBorrowers = new();

    internal static GrpcClientManager GrpcMgr = null!;

    private void Awake()
    {
        Instance = this;
        Logger = base.Logger;
        
        PluginConfig.InitSettings(Config);
        MissionVote = new MissionVoteService(Config);
        Logger.LogInfo("Loaded MissionVote");
        
        WarnService = new WarnService(Config);
        Logger.LogInfo("Loaded WarnService");
        
        WeatherRandomizer = new WeatherRandomizer(Config);
        Logger.LogInfo("Loaded WeatherRandomizer");

        MissionBalance = new MissionBalanceService();
        
        try
        {
            PlayerIdentifier = new PlayerIdentificationService();
            Logger.LogInfo("Loaded PlayerID");
        } catch (Exception e)
        {
            Logger.LogDebug($"Failed loading PlayerID with exception {e}");
        }
        
        Logger.LogInfo($"Loading {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION}...");
        
        // TimeService.Initialize();

        RestartWarningService.ScheduleWarnings();
        
        PatchAll();
        
        // Load all Commands (Inheritors of PermissionConfigurableCommand) using Reflection.
        {
            var assembly = Assembly.GetExecutingAssembly();

            var commandTypes = assembly.GetTypes()
                .Where(t => t.IsClass
                            && !t.IsAbstract
                            && t.IsSubclassOf(typeof(PermissionConfigurableCommand)));

            foreach (var type in commandTypes)
            {
                try
                {
                    var commandInstance = (PermissionConfigurableCommand)Activator.CreateInstance(type, Config);

                    CommandService.AddCommand(commandInstance);
                    Logger.LogInfo($"Loaded command {type.Name}");
                }
                catch (Exception ex)
                {
                    // It's good practice to log this in BepInEx so one broken command doesn't break them all
                    Logger.LogError($"Failed to load command {type.Name}: {ex.Message}");
                }
            }
        }
        
        PlayerEvents.PlayerLeft += OnPlayerLeave;
        PlayerEvents.PlayerLeft += _ => MissionBalance.CheckAndApplyBalance();
        PlayerEvents.PlayerJoined += OnPlayerJoin;
        PlayerEvents.PlayerJoined += MissionBalanceService.OnPlayerJoin;
        PlayerEvents.PlayerJoinedFaction += OnPlayerJoinFaction;
        PlayerEvents.PlayerJoinedFaction += (_, _) => MissionBalance.CheckAndApplyBalance();

        MissionEvents.MissionLoaded += m => MissionBalance.OnMissionLoad(m);
        MissionEvents.MissionLoaded += _ => MissionVote.ClearInhibit();

        TimeEvents.Every10Minutes += BroadcastService.SendBroadcast;
        
        TimeService.Initialize();
        
        GrpcMgr = new GrpcClientManager(Config);
    }

    private static void PatchAll()
    {
        if (IsPatched)
        {
            Logger.LogWarning("Already patched!");
            return;
        }

        Logger.LogDebug("Patching...");

        Harmony ??= new Harmony(PluginInfo.PLUGIN_GUID);

        try
        {
            Harmony.PatchAll();
            IsPatched = true;
            Logger.LogDebug("Patched!");
        }
        catch (Exception e)
        {
            Logger.LogError($"Aborting server launch: Failed to Harmony patch the game. Error trace:\n{e}");
        }
    }
    
    [UsedImplicitly]
    private void UnpatchSelf()
    {
        if (Harmony == null)
        {
            Logger.LogError("Harmony instance is null!");
            return;
        }
        
        if (!IsPatched)
        {
            Logger.LogWarning("Already unpatched!");
            return;
        }

        Logger.LogDebug("Unpatching...");

        Harmony.UnpatchSelf();
        IsPatched = false;

        Logger.LogDebug("Unpatched!");
    }
    
    private static void OnPlayerJoin(Player player)
    {
        if (CheckOwnerBanned(player))
        {
            PlayerUtils.KickPlayer(player, "The owner of this familyshared account is banned.");
            return;
        }
        
        if (StaffSlotService.IsSlotStaff(Globals.DedicatedServerManagerInstance.RealPlayerCount()) && !PlayerUtils.IsStaff(player))
        {
            Globals
                .NetworkManagerNuclearOptionInstance
                .KickPlayerAsync(
                    player,
                    $"This slot is reserved for staff. The max capacity is {Globals.DedicatedServerManagerInstance.Config.MaxPlayers}.",
                    false)
                .Forget();
            return;
        }
        
        Logger.LogDebug($"{player.PlayerName} : {player.SteamID} - joined the game");
        var originalName = player.PlayerName;
        PlayerUtils.ApplyOrRemoveStaffTag(player);
        // Apply identification tag if not a staff member
        if (!PlayerUtils.IsStaff(player))
        {
            PlayerIdentifier.AssignNewPlayer(player);
            PlayerUtils.ApplyIdentificationTag(player, PlayerIdentifier.GetPlayerId(player));
        }
        Logger.LogInfo($"{player.PlayerName} : {player.SteamID} - joined the game");
        var log = new JoinLeaveLog
        {
            SteamID = player.SteamID,
            IsOn = true,
            Name = originalName,
            Time = DateTime.UtcNow.ToTimestamp()
        };
        GrpcMgr.Client?.SendPlayerActivityAsync(log);
    }

    private static void OnPlayerLeave(Player player)
    {
        Logger.LogInfo($"{player.PlayerName} : {player.SteamID} - left the game");
        MissionVote.RemoveVoter(player.SteamID);
        PlayerIdentifier.RemovePlayer(player);
        var log = new JoinLeaveLog
        {
            SteamID = player.SteamID,
            IsOn = false,
            Name = player.PlayerName,
            Time = DateTime.UtcNow.ToTimestamp(),
            Score = (float)Math.Round(player.PlayerScore, 2)
        };
        GrpcMgr.Client?.SendPlayerActivityAsync(log);
    }

    private static void OnPlayerJoinFaction(Player player, FactionHQ HQ)
    {
        var log = new FactionLog
        {
            SteamID = player.SteamID,
            Faction = HQ.faction.name
        };
        GrpcMgr.Client?.SendPlayerJoinFacAsync(log);
    }

    private static bool CheckOwnerBanned(Player player)
    {
        if (!FamilySharingBorrowers.TryGetValue(player.SteamID, out var ownerSteamID)) return false;
        return Globals.NetworkManagerNuclearOptionInstance.Authenticator.BanList.Contains(new CSteamID(ownerSteamID));
    }

    internal static void OnPlayerTeamkill(Player killer, Player killed, string weaponName)
    {
        OnTeamkill(killer, killed.PlayerName, weaponName);
    }

    /// <summary>
    /// Method for handling player teamkills
    /// </summary>
    /// <param name="killer">Player that teamkilled something</param>
    /// <param name="killedName">name of the thing that was killed</param>
    /// <param name="weaponName">name of the used weapon.</param>
    public static void OnTeamkill(Player killer, string killedName, string weaponName)
    {
        if (!PluginConfig.EnableTeamDamageAutoWarning!.Value) return;
        var reason = $"Teamkilled player {killedName} with weapon {weaponName}";
        WarnService.AddWarn(killer.SteamID, reason);
    }
}