using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Utils;
using GW_server_plugin.Features.CommandUtils;
using GW_server_plugin.Helpers;
using GW_server_plugin.Patches;
using UnityEngine;

namespace GW_server_plugin.Features.Protobuf_IPC;

/// <summary>
/// Manages the embedded plugin GRPC client for the graywar NOServerManager 
/// </summary>
public class GrpcClientManager
{
    private readonly ConfigEntry<string> _serverName;
    private readonly ConfigEntry<string> _centralHost;
    private readonly ConfigEntry<uint> _centralPort;
    
    private readonly HashSet<ulong> _logSuppressedSteamIDs = [];

    internal EdgeAgentService.EdgeAgentServiceClient? Client;
    internal IClientStreamWriter<ChatLog>? ChatLogStream;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="config"></param>
    public GrpcClientManager(ConfigFile config)
    {
        var enable = config.Bind(PluginConfig.RpcSection, "enable", true);
        _serverName = config.Bind(PluginConfig.RpcSection, "server name", "graywar",
            "Name the server will report to the manager");
        _centralHost = config.Bind(PluginConfig.RpcSection, "central hostname", "graywar.no",
            "Hostname or IP of the manager");
        _centralPort = config.Bind(PluginConfig.RpcSection, "central port", 50051u,
            new ConfigDescription("Port of the manager", new AcceptableValueRange<uint>(0, 65535)));
        if (enable.Value)
            InitializeGrpc();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private void InitializeGrpc()
    {
        ChannelCredentials creds = new SslCredentials(
            File.ReadAllText("CA/ca.crt"),
            new KeyCertificatePair(
                File.ReadAllText($"CA/{_serverName.Value}.crt"),
                File.ReadAllText($"CA/{_serverName.Value}.key")
            )
        );
        var channel = new Channel(_centralHost.Value, Convert.ToInt32(_centralPort.Value), creds);
        Client = new EdgeAgentService.EdgeAgentServiceClient(channel);
        var chatStream = Client.SendChatLogsStream();
        ChatLogStream = chatStream.RequestStream;
        
        BanInputBehaviour(Client.SubscribeToBans(new Empty()));
        CommandBehaviour(Client.SubscribeToCommands());
        StatusRequestBehaviour(Client.StatusStream());
        ProcessDiscordMessages(chatStream.ResponseStream);
    }

    private void CommandBehaviour(AsyncDuplexStreamingCall<CommandResult, Command> stream)
    {
        stream.ResponseStream.ForEachAsync(async data =>
        {
            if (!data.Result)
            {
                _ = CommandService.TryExecuteCommand(data.Name, data.Arguments.ToArray(), data.PermLevel);
                return;
            } 
            var result = await CommandService.TryExecuteCommand(data.Name, data.Arguments.ToArray(), data.PermLevel);
            await stream.RequestStream.WriteAsync(new CommandResult
            {
                RequestID = data.RequestID,
                Ok = result.success,
                Result = result.response
            });
        });
    }

    private void BanInputBehaviour(AsyncServerStreamingCall<BanRequest> stream)
    {
        stream.ResponseStream.ForEachAsync(data =>
        {
            try
            {
                _ = data.ShouldBeBanned
                    ? CommandService.TryExecuteCommand("ban", [data.SteamID.ToString(), data.Reason], PermissionLevel.Admin) 
                    : CommandService.TryExecuteCommand("unban", [data.SteamID.ToString()], PermissionLevel.Admin);
                _logSuppressedSteamIDs.Add(data.SteamID);
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        });
    }
    
    private static void StatusRequestBehaviour(AsyncDuplexStreamingCall<StatusResponse, StatusRequest> stream)
    {
        stream.ResponseStream.ForEachAsync(async data =>
            {
                var missionName = Globals.DedicatedServerManagerInstance.currentMissionOption.Key.Name ?? 
                                  Globals.DedicatedServerManagerInstance.NextMissionOption.Key.Name;
                if (ulong.TryParse(missionName, out var id))
                {
                    var missionNameResult = await MissionNameFix.GetMissionNameAsync(id);
                    if (missionNameResult.success)
                    {
                        missionName = missionNameResult.name!;
                    }
                }
                
                StatusResponse rt;
                try
                {
                    rt = new StatusResponse
                    {
                        Ok = true,
                        RequestID = data.RequestID,
                        MaxPlayers = (uint)Globals.NetworkManagerNuclearOptionInstance.Server.PeerConfig.MaxConnections,
                        PlayerNumber = (uint)PlayerUtils.GetPlayerCount(),
                        MissionName = missionName ?? "Not started",
                        MissionStart = DateTime.UtcNow.AddSeconds(-MissionService.CurrentMissionTime).ToTimestamp(),
                        LastRestart = DateTime.UtcNow.AddSeconds(-Time.realtimeSinceStartup).ToTimestamp()
                    };
                }
                catch (Exception e)
                {
                    GwServerPlugin.Logger.LogError(e.ToString());
                    rt = new StatusResponse
                    {
                        Ok = false
                    };
                }
                await stream.RequestStream.WriteAsync(rt);
            }
        );
    }
    
    private void ProcessDiscordMessages(IAsyncStreamReader<ChatBack> inputStream)
    {
        inputStream.ForEachAsync(data =>
            {
                try
                {
                    var text = $"<color=#5865F2>[DC]</color> {data.SenderName}: {data.Message}";
                    Globals.ChatManagerInstance.RpcServerMessage(text, true);
                    return Task.CompletedTask;
                }
                catch (Exception exception)
                {
                    return Task.FromException(exception);
                }
            }
        );
    }
    
    internal void TrySendBan(BanRequest banLog)
    {
        if (Client == null) return;
        if (_logSuppressedSteamIDs.Contains(banLog.SteamID))
        {
            _logSuppressedSteamIDs.Remove(banLog.SteamID);
            return;
        }
        Client.SendBanAsync(banLog);
    }

}