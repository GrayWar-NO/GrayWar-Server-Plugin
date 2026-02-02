using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BepInEx;
using BepInEx.Logging;
using GW_server_plugin.Events;
using GW_server_plugin.Features;
using HarmonyLib;
using NuclearOption.Networking;
using Steamworks;
using UnityEngine;
using Random = System.Random;

namespace GW_server_plugin;

/// <summary>
/// Main plugin class for the plugin
/// </summary>
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class GwServerPlugin : BaseUnityPlugin
{
    internal static GwServerPlugin? Instance { get; private set; }
    internal new static ManualLogSource? Logger { get; private set; }
    private static Harmony? Harmony { get; set; }
    private static bool IsPatched { get; set; }

    private ConcurrentQueue<string> socketInbox = new();

    private IpcSocket socket;
    
    private void Awake()
    {
        Instance = this;
        
        Logger = base.Logger;
        Logger.LogInfo($"Loading {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION}...");
        
        PluginConfig.InitSettings(Config);
        TimeService.Initialize();


        socket = new IpcSocket();
        socket.OnJson += msg => socketInbox.Enqueue(msg);
        socket.Start(PluginConfig.IpcHost!.Value, PluginConfig.IpcPort!.Value);

        TimeEvents.EverySecond += EverySecond;

    }

    private void EverySecond()
    {
        while (socketInbox.TryDequeue(out var msg))
        {
            socket?.SendJson(msg);
        }
    }
    
}