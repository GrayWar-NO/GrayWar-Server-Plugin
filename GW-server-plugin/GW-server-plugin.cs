using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using Com.Graywar.NoServerManager.Proto;
using Google.Protobuf.WellKnownTypes;
using GW_server_plugin.Events;
using GW_server_plugin.Features;
using GW_server_plugin.Features.CommandUtils;
using GW_server_plugin.Features.Protobuf_IPC;
using GW_server_plugin.Features.Voting;
using GW_server_plugin.Helpers;
using GW_server_plugin.Patches.KillsLogging;
using HarmonyLib;
using JetBrains.Annotations; using NuclearOption.Networking;
using Steamworks;

namespace GW_server_plugin;

/// <summary>
/// Main plugin class
/// </summary>
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class GwServerPlugin : BaseUnityPlugin
{
    internal static readonly CancellationTokenSource shutdownCts = new();
    
    internal static GwServerPlugin Instance { get; private set; } = null!;
    
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static PlayerIdentificationService PlayerIdentifier { get; private set; } = null!;
    
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
    
    internal static DateTime ServerStartTime; // Used to restart server over 24 hours

    internal static GrpcClientManager GrpcMgr = null!;

    /// <summary>
    /// Maps each connected player's SteamID to their current display name.
    /// </summary>
    private static readonly Dictionary<ulong, string> ConnectedPlayerNames = [];

    private static readonly object ConnectedPlayerNamesLock = new();

    internal static bool TryGetConnectedPlayerSteamId(string playerName, out ulong steamId)
    {
        lock (ConnectedPlayerNamesLock)
        {
            var player = ConnectedPlayerNames.FirstOrDefault(pair =>
                string.Equals(pair.Value, playerName, StringComparison.CurrentCultureIgnoreCase));
            steamId = player.Key;
            return player.Key != 0;
        }
    }

    internal static bool TryGetConnectedPlayerName(ulong steamId, out string playerName)
    {
        lock (ConnectedPlayerNamesLock)
        {
            return ConnectedPlayerNames.TryGetValue(steamId, out playerName!);
        }
    }

    private void Awake()
    {
        ServerStartTime = DateTime.Now;
        Instance = this;
        Logger = base.Logger;
        
        PluginConfig.InitSettings(Config);
        
        WarnService = new WarnService(Config);
        Logger.LogInfo("Loaded WarnService");
        
        WeatherRandomizer = new WeatherRandomizer(Config);
        Logger.LogInfo("Loaded WeatherRandomizer");

        MissionBalance = new MissionBalanceService();
        Logger.LogInfo("Loaded MissionBalanceService");
        
        RestartService.Initialize(Config);
        Logger.LogInfo("Initialized RestartService");
        
        RankCatchUpService.Initialize(Config);
        Logger.LogInfo("Initialized RankCatchUpService");
        
        VoteManager.Initialize(Config);
        Logger.LogInfo("Initialized VoteManager");
        
        try
        {
            PlayerIdentifier = new PlayerIdentificationService();
            Logger.LogInfo("Loaded PlayerID");
        } catch (Exception e)
        {
            Logger.LogDebug($"Failed loading PlayerID with exception {e}");
        }
        
        Logger.LogInfo($"Loading {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION}...");
        
        PatchAll();
        
        // Load all Commands (Inheritors of PermissionConfigurableCommand) using Reflection.
        {
            var assembly = Assembly.GetExecutingAssembly();

            var commandTypes = assembly.GetTypes()
                .Where(t => t.IsClass
                            && !t.IsAbstract
                            && t.IsSubclassOf(typeof(ConfigurableCommand)));

            foreach (var type in commandTypes)
            {
                try
                {
                    var commandInstance = (ConfigurableCommand)Activator.CreateInstance(type, Config);

                    if (!commandInstance.Enable) continue;

                    CommandService.AddCommand(commandInstance);
                    Logger.LogInfo($"Loaded command {type.Name}");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to load command {type.Name}: {ex.Message}");
                }
            }
        }
        
        PlayerEvents.PlayerLeft += OnPlayerLeave;
        PlayerEvents.PlayerLeft += _ => MissionBalance.CheckAndApplyBalance();
        PlayerEvents.PlayerJoined += OnPlayerJoin;
        PlayerEvents.PlayerJoined += MissionBalanceService.OnPlayerJoin;
        PlayerEvents.PlayerJoined += _ => RestartService.CancelRestart();
        PlayerEvents.PlayerJoinedFaction += OnPlayerJoinFaction;
        PlayerEvents.PlayerJoinedFaction += (_, _) => MissionBalance.CheckAndApplyBalance();

        MissionEvents.MissionLoaded += m => MissionBalance.OnMissionLoad(m);
        MissionEvents.MissionLoaded += _ => VoteManager.RemoveInhibit(MissionService.VoteInhibitionReason);
        
        MissionEvents.MissionEnded += _ => VoteManager.Inhibit(MissionService.VoteInhibitionReason);

        TimeEvents.Every10Minutes += BroadcastService.SendBroadcast;
        
        TimeEvents.Every30Minutes += RestartService.AutoRestart;
        
        TimeService.Initialize();
        do
        {
            try
            {
                GrpcMgr = new GrpcClientManager(Config);
                if (GrpcMgr.Client == null)
                {
                    Logger.LogInfo("gRPC manager did not initialize: is disabled.");
                    break;
                }
                var modList = GrpcMgr.Client.getStaffList(new Empty())!;
                PluginConfig.UpdateModList(modList);

                var bans = GrpcMgr.Client.GetBanList(new Empty()).Bans
                    .Select(ban => (id: new CSteamID(ban.SteamID), reason: ban.Reason));

                _ = UpdateBanListWhenReadyAsync(bans);
                
                Logger.LogInfo("gRPC interface started!");
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to initialize GrpcClientManager: {e}\n{e.StackTrace}");
            }
        } while (false); // Do - while false block is used to have a clean way to exit the try block directly.
        
    }
    
    private void OnDestroy()
    {
        shutdownCts.Cancel();
        shutdownCts.Dispose();
    }
    
    private static async Task UpdateBanListWhenReadyAsync(IEnumerable<(CSteamID id, string reason)> bans)
    {
        while (Globals.NetworkManagerNuclearOptionInstance == null ||
               Globals.DedicatedServerManagerInstance == null)
        {
            await Task.Delay(1000);
        }
        
        AllowBanListUtils.ReplaceWithNewData(
            Globals.NetworkManagerNuclearOptionInstance.Authenticator.BanList,
            Globals.DedicatedServerManagerInstance.Config.BanListPaths[0],
            bans.ToList()
        );
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
        
        var originalName = player.GetPlayerName().SanitizedName;
        lock (ConnectedPlayerNamesLock)
        {
            ConnectedPlayerNames[player.SteamID] = originalName;
        }
        // Assign an ID used by PlayerUtils.GetDisplayName for non-staff players.
        if (!PlayerUtils.IsStaff(player))
        {
            PlayerIdentifier.AssignNewPlayer(player);
        }

        _ = UpdateConnectedPlayerNameAsync(player, DateTime.UtcNow);
        
    }

    private static void OnPlayerLeave(Player player)
    {
        var logName = player.GetLogName();
        lock (ConnectedPlayerNamesLock)
        {
            ConnectedPlayerNames.Remove(player.SteamID);
        }

        Logger.LogInfo($"{logName} : {player.SteamID} - left the game");
        VoteManager.Session?.RemoveVoter(player);
        PlayerIdentifier.RemovePlayer(player);
        var log = new JoinLeaveLog
        {
            SteamID = player.SteamID,
            IsOn = false,
            Name = logName,
            Time = DateTime.UtcNow.ToTimestamp(),
            Score = (float)Math.Round(player.PlayerScore, 2)
        };
        GrpcMgr.Client?.SendPlayerActivityAsync(log);
        RestartService.CheckIfNoPlayers();
    }

    private static async Task UpdateConnectedPlayerNameAsync(Player player, DateTime joinedAt)
    {
        var steamId = player.SteamID;
        var username = await SteamWebApi.GetUsernameAsync(steamId).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(username)) return;

        var playerIsStillConnected = false;
        lock (ConnectedPlayerNamesLock)
        {
            // Do not re-add a player who left while the web request was pending.
            if (ConnectedPlayerNames.ContainsKey(steamId))
            {
                ConnectedPlayerNames[steamId] = username!;
                playerIsStillConnected = true;
            }
        }

        if (!playerIsStillConnected) return;

        var logName = player.GetLogName();
        Logger.LogInfo($"{logName} : {steamId} - joined the game");
        var log = new JoinLeaveLog
        {
            SteamID = steamId,
            IsOn = true,
            Name = logName,
            Time = joinedAt.ToTimestamp()
        };
        GrpcMgr.Client?.SendPlayerActivityAsync(log);
    }

    // ReSharper disable once InconsistentNaming
    private static void OnPlayerJoinFaction(Player player, FactionHQ HQ)
    {
        Logger.LogInfo($"{player.SteamID} joined {HQ.faction.factionName}");
        var log = new FactionLog
        {
            SteamID = player.SteamID,
            Faction = HQ.faction.name
        };
        GrpcMgr.Client?.SendPlayerJoinFacAsync(log);
    }
    
    internal static void OnPlayerTeamkill(Player killer, Player killed, string weaponName)
    {
        OnTeamkill(killer, killed.GetLogName(), weaponName);
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
