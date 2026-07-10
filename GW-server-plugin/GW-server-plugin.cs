using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using GW_server_plugin.Enums;
using GW_server_plugin.Events;
using GW_server_plugin.Features;
using GW_server_plugin.Features.CommandUtils;
using GW_server_plugin.Features.IPC;
using GW_server_plugin.Features.IPC.Packets;
using GW_server_plugin.Features.Protobuf_IPC;
using GW_server_plugin.Helpers;
using GW_server_plugin.Patches.KillsLogging;
using HarmonyLib;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NuclearOption.Networking;
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

    /// <summary>
    /// Logging Outbox for the IPC communication and general logging to a file
    /// </summary>
    public static BlockingCollection<CommunicationPacket> LoggingOutBox = new();

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
    
    private CancellationTokenSource? _cts;
    
    internal static readonly Dictionary<ulong, ulong> FamilySharingBorrowers = new();


    private Socket? _socket;
    
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


        if (PluginConfig.IpcEnable!.Value) {
            _socket = new Socket();
            _socket.OnJson += HandleJson;
            _socket.Start(PluginConfig.IpcHost!.Value, PluginConfig.IpcPort!.Value);
            StartLoggingSender();
        }
        
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

        try
        {
            var grpcMgr = new GrpcClientManager(Config);
        }
        catch (Exception e)
        {
            Logger.LogDebug($"Error initializing grpc client: {e}\n{e.StackTrace}");
        }
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



    private void StartLoggingSender()
    {
        _cts = new CancellationTokenSource();

        Task.Run(() =>
        {
            try
            {
                foreach (var msg in LoggingOutBox.GetConsumingEnumerable(_cts.Token))
                {
                    _socket?.SendJson(JsonConvert.SerializeObject(msg)); // Null if IPC is not enabled.
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown
            }
        });
    }
    
    private static async void HandleJson(string msg)
    {
        try
        {
            var packet = JsonConvert.DeserializeObject<CommunicationPacket>(msg);
            CommunicationPacket? respPacket = await packet!.Process();
            if (respPacket is null) return;
            LoggingOutBox.Add(respPacket);
        }
        catch (Exception e)
        {
            Logger.LogError($"Error when recieving Json in IPC: {e}");
        }
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
        var joinPacket = new LogEntryPacket
        {
            Channel = LogChannel.JoinLeave,
            LogText = $"1:{player.SteamID}:{originalName}"
        };
        LoggingOutBox.Add(joinPacket);
    }

    private static void OnPlayerLeave(Player player)
    {
        Logger.LogInfo($"{player.PlayerName} : {player.SteamID} - left the game");
        MissionVote.RemoveVoter(player.SteamID);
        PlayerIdentifier.RemovePlayer(player);
        var leavePacket = new LogEntryPacket
        {
            Channel = LogChannel.JoinLeave,
            LogText = $"0:{player.SteamID}:{Math.Round(player.PlayerScore, 2)}"
        };
        LoggingOutBox.Add(leavePacket);
    }

    private static void OnPlayerJoinFaction(Player player, FactionHQ HQ)
    {
        var factionJoinPacket = new LogEntryPacket
        {
            Channel = LogChannel.FactionJoin,
            LogText = $"{player.SteamID}:{HQ.faction.name}"
        };
        LoggingOutBox.Add(factionJoinPacket);
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