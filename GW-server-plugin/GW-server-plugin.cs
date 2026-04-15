using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using GW_server_plugin.Enums;
using GW_server_plugin.Events;
using GW_server_plugin.Features;
using GW_server_plugin.Features.CommandUtils;
using GW_server_plugin.Features.CommandUtils.Commands;
using GW_server_plugin.Features.IPC;
using GW_server_plugin.Features.IPC.Packets;
using GW_server_plugin.Helpers;
using GW_server_plugin.Patches.KillsLogging;
using HarmonyLib;
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
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static PlayerIdentificationService PlayerIdentifier { get; private set; } = null!;

    /// <summary>
    /// Logging Outbox for the IPC communication and general logging to a file
    /// </summary>
    public static BlockingCollection<CommunicationPacket> LoggingOutBox = new();

    internal static MissionVoteService MissionVote { get; private set; } = null!;

    internal static VoteKickService VoteKickService { get; private set; } = null!;

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
        Logger = base.Logger;
        
        PluginConfig.InitSettings(Config);
        MissionVote = new MissionVoteService(Config);
        Logger.LogInfo("Loaded MissionVote");
        
        WarnService = new WarnService(Config);
        Logger.LogInfo("Loaded WarnService");
        
        VoteKickService = new VoteKickService();
        Logger.LogInfo("Loaded VoteKick");
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
        
        PatchAll();
        
        CommandService.AddCommand(new TellCommand(Config));
        CommandService.AddCommand(new HelpCommand(Config));
        CommandService.AddCommand(new ListPlayersCommand(Config));
        CommandService.AddCommand(new SetPermissionLevelCommand(Config));
        CommandService.AddCommand(new AddSlotCommand(Config));
        CommandService.AddCommand(new RemoveSlotCommand(Config));
        
        CommandService.AddCommand(new ListMissionsCommand(Config));
        CommandService.AddCommand(new NextMissionCommand(Config));
        CommandService.AddCommand(new RtvCommand(Config));
        
        CommandService.AddCommand(new WarnCommand(Config));
        
        CommandService.AddCommand(new VoteKickCommand(Config));
        CommandService.AddCommand(new KickCommand(Config));
        CommandService.AddCommand(new UnKickCommand(Config));
        CommandService.AddCommand(new ClearKickListCommand(Config));
        
        CommandService.AddCommand(new BanCommand(Config));
        CommandService.AddCommand(new UnbanCommand(Config));
        
        CommandService.AddCommand(new LinkmeCommand(Config));

        PlayerEvents.PlayerLeft += OnPlayerLeave;
        PlayerEvents.PlayerJoined += OnPlayerJoin;

        TimeEvents.Every10Minutes += BroadcastService.SendBroadcast;
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
    private void HandleJson(string msg)
    {
        var packet = JsonConvert.DeserializeObject<CommunicationPacket>(msg);
        CommunicationPacket? respPacket = packet!.Process();
        if (respPacket is null) return;
        LoggingOutBox.Add(respPacket);
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
            Globals.NetworkManagerNuclearOptionInstance.KickPlayerAsync(player, $"This slot is reserved for staff. The max capacity is {Globals.DedicatedServerManagerInstance.Config.MaxPlayers}.").Forget();
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
        VoteKickService.RemoveVoter(player.SteamID);
        PlayerIdentifier.RemovePlayer(player);
        var leavePacket = new LogEntryPacket
        {
            Channel = LogChannel.JoinLeave,
            LogText = $"0:{player.SteamID}:{Math.Round(player.PlayerScore, 2)}"
        };
        LoggingOutBox.Add(leavePacket);
    }

    private static bool CheckOwnerBanned(Player player)
    {
        if (!FamilySharingBorrowers.TryGetValue(player.SteamID, out var ownerSteamID)) return false;
        return Globals.NetworkManagerNuclearOptionInstance.Authenticator.BanList.Contains(new CSteamID(ownerSteamID));
    }

    /// <summary>
    /// Method for handling teamkills
    /// </summary>
    /// <param name="killer"></param>
    /// <param name="killed"></param>
    /// <param name="weaponName"></param>
    public static void OnTeamkill(Player killer, Player killed, string weaponName)
    {
        if (weaponName.Contains("kt")) return;// if weapon is a nuke
        var warnLogPacket = new LogEntryPacket
        {
            LogText = $"{killer.SteamID}:Teamkilled player {killed.PlayerName} with weapon {weaponName}",
            Channel = LogChannel.Warn
        };
        LoggingOutBox.Add(warnLogPacket);
        if (!PluginConfig.EnableTeamDamageAutoWarning!.Value) return;
        WarnService.AddWarn(killer.SteamID);
    }
}