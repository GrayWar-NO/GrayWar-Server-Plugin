using System;
using System.Collections.Concurrent;
using BepInEx;
using BepInEx.Logging;
using GW_server_plugin.Events;
using GW_server_plugin.Features;
using GW_server_plugin.Features.CommandUtils;
using GW_server_plugin.Features.CommandUtils.Commands;
using GW_server_plugin.Features.IPC;
using GW_server_plugin.Features.IPC.Packets;
using GW_server_plugin.Helpers;
using HarmonyLib;
using Newtonsoft.Json;
using NuclearOption.Networking;

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
    /// Socket Outbox for the IPC communication
    /// </summary>
    internal static readonly ConcurrentQueue<string> SocketOutBox = new();

    internal static MissionVoteService MissionVote { get; private set; } = null!;

    private static Harmony? Harmony { get; set; }
    private static bool IsPatched { get; set; }


    private Socket? _socket;
    
    private void Awake()
    {
        Logger = base.Logger;

        MissionVote = new MissionVoteService(Config);

        PlayerIdentifier = new PlayerIdentificationService();

        
        Logger.LogInfo($"Loading {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION}...");
        
        PluginConfig.InitSettings(Config);
        TimeService.Initialize();


        if (PluginConfig.IpcEnable!.Value) {
            _socket = new Socket();
            _socket.OnJson += HandleJson;
            _socket.Start(PluginConfig.IpcHost!.Value, PluginConfig.IpcPort!.Value);
        }
        
        PatchAll();
        
        CommandService.AddCommand(new TellCommand(Config));
        CommandService.AddCommand(new HelpCommand(Config));
        CommandService.AddCommand(new ListPlayersCommand(Config));
        
        CommandService.AddCommand(new ListMissionsCommand(Config));
        CommandService.AddCommand(new NextMissionCommand(Config));
        CommandService.AddCommand(new RtvCommand(Config));
        
        CommandService.AddCommand(new WarnCommand(Config));
        
        CommandService.AddCommand(new KickCommand(Config));
        CommandService.AddCommand(new UnKickCommand(Config));
        CommandService.AddCommand(new ClearKickListCommand(Config));
        
        CommandService.AddCommand(new BanCommand(Config));
        CommandService.AddCommand(new UnbanCommand(Config));

        TimeEvents.EverySecond += EverySecond;
        
        PlayerEvents.PlayerLeft += OnPlayerLeave;
        PlayerEvents.PlayerJoined += OnPlayerJoin;
        
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


    private void EverySecond()
    {
        while (SocketOutBox.TryDequeue(out var msg))
        {
            _socket?.SendJson(msg);
        }
    }

    private void HandleJson(string msg)
    {
        var packet = JsonConvert.DeserializeObject<CommunicationPacket>(msg);
        CommunicationPacket? respPacket = packet!.Process();
        if (respPacket is null) return;
        SocketOutBox.Enqueue(JsonConvert.SerializeObject(respPacket));
    }
    private static void OnPlayerJoin(Player player)
    {
        PlayerUtils.ApplyOrRemoveStaffTag(player);
        PlayerIdentifier.AssignNewPlayer(player);
        // Apply identification tag if not a staff member
        if (!PluginConfig.IsAdmin(player.SteamID) && !PluginConfig.IsModerator(player.SteamID) &&
            !PluginConfig.IsOwner(player.SteamID))
        {
            PlayerUtils.ApplyIdentificationTag(player, PlayerIdentifier.GetPlayerId(player));
        }
        Logger.LogInfo($"{player.PlayerName} : {player.SteamID} - joined the game");
    }

    private static void OnPlayerLeave(Player player)
    {
        Logger.LogInfo($"{player.PlayerName} : {player.SteamID} - left the game");
        MissionVote.RemoveVoter(player.SteamID);
        PlayerIdentifier.RemovePlayer(player);
    }


}