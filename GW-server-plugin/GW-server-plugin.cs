using System;
using System.Collections.Concurrent;
using BepInEx;
using BepInEx.Logging;
using GW_server_plugin.Events;
using GW_server_plugin.Features;
using GW_server_plugin.Features.Commands;
using GW_server_plugin.Features.CommandUtils.Commands;
using GW_server_plugin.Features.IPC;
using GW_server_plugin.Features.IPC.Packets;
using HarmonyLib;
using Newtonsoft.Json;

namespace GW_server_plugin;

/// <summary>
/// Main plugin class for the plugin
/// </summary>
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class GwServerPlugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static PlayerIdentificationService PlayerIdentifier { get; private set; } = null!;

    private readonly ConcurrentQueue<string> _socketOutBox = new();

    private static Harmony? Harmony { get; set; }
    private static bool IsPatched { get; set; }


    private Socket? _socket;
    
    private void Awake()
    {
        Logger = base.Logger;

        PlayerIdentifier = new PlayerIdentificationService();

        
        Logger.LogInfo($"Loading {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION}...");
        
        PluginConfig.InitSettings(Config);
        TimeService.Initialize();


        if (PluginConfig.IpcEnable!.Value) {
            _socket = new Socket();
            _socket.OnJson += HandleJson;
            _socket.Start(PluginConfig.IpcHost!.Value, PluginConfig.IpcPort!.Value);
        }
        TimeEvents.EverySecond += EverySecond;
        
        PatchAll();
        
        CommandService.AddCommand(new TellCommand(Config));
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
        while (_socketOutBox.TryDequeue(out var msg))
        {
            _socket?.SendJson(msg);
        }
    }

    private void HandleJson(string msg)
    {
        var packet = JsonConvert.DeserializeObject<CommunicationPacket>(msg);
        CommunicationPacket? respPacket = packet!.Process();
        if (respPacket is null) return;
        _socketOutBox.Enqueue(JsonConvert.SerializeObject(respPacket));
    }
    
}