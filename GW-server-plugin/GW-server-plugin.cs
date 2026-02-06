using System.Collections.Concurrent;
using BepInEx;
using BepInEx.Logging;
using GW_server_plugin.Events;
using GW_server_plugin.Features;
using GW_server_plugin.Features.IPC;
using GW_server_plugin.Features.IPC.Packets;
using Newtonsoft.Json;

namespace GW_server_plugin;

/// <summary>
/// Main plugin class for the plugin
/// </summary>
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class GwServerPlugin : BaseUnityPlugin
{
    internal static GwServerPlugin? Instance { get; private set; }
    internal new static ManualLogSource? Logger { get; private set; }

    private readonly ConcurrentQueue<string> _socketOutBox = new();

    private Socket? _socket;
    
    private void Awake()
    {
        Instance = this;
        
        Logger = base.Logger;
        Logger.LogInfo($"Loading {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION}...");
        
        PluginConfig.InitSettings(Config);
        TimeService.Initialize();


        _socket = new Socket();
        _socket.OnJson += HandleMsg;
        _socket.Start(PluginConfig.IpcHost!.Value, PluginConfig.IpcPort!.Value);

        TimeEvents.EverySecond += EverySecond;

    }

    private void EverySecond()
    {
        while (_socketOutBox.TryDequeue(out var msg))
        {
            _socket?.SendJson(msg);
        }
    }

    private void HandleMsg(string msg)
    {
        var packet = JsonConvert.DeserializeObject<CommunicationPacket>(msg);
        CommunicationPacket? respPacket = packet!.Process();
        if (respPacket is null) return;
        _socketOutBox.Enqueue(JsonConvert.SerializeObject(respPacket));
    }
    
}