using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Utils;
using GW_server_plugin.Features.CommandUtils;
using GW_server_plugin.Helpers;
using Steamworks;
using UnityEngine;

namespace GW_server_plugin.Features.Protobuf_IPC;

/// <summary>
/// Manages the embedded plugin GRPC client for the graywar NOServerManager 
/// </summary>
public class GrpcClientManager
{
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly ConfigEntry<string> _serverName;
    private readonly ConfigEntry<string> _centralHost;
    private readonly ConfigEntry<uint> _centralPort;
    private ChannelCredentials? _sslCredentials;

    private CancellationTokenSource? _monitorCts;

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

        if (!enable.Value) return;
        _sslCredentials = new SslCredentials(
            File.ReadAllText("CA/ca.crt"),
            new KeyCertificatePair(
                File.ReadAllText($"CA/{_serverName.Value}.crt"),
                File.ReadAllText($"CA/{_serverName.Value}.key")
            )
        );
        ConnectAndMonitor();
    }

    private async void ConnectAndMonitor()
    {
        try
        {
            _monitorCts?.Cancel();
            _monitorCts = new CancellationTokenSource();
            var token = _monitorCts.Token;

            var channel = new Channel(_centralHost.Value, Convert.ToInt32(_centralPort.Value), _sslCredentials);
            Connect(channel);
            var lastState = channel.State;

            while (!token.IsCancellationRequested)
            {
                await channel.WaitForStateChangedAsync(lastState);
                lastState = channel.State;

                GwServerPlugin.Logger.LogDebug($"gRPC Channel state changed to: {lastState}");


                while (!token.IsCancellationRequested)
                {
                    await channel.WaitForStateChangedAsync(lastState);
                    lastState = channel.State;

                    GwServerPlugin.Logger.LogDebug($"gRPC Channel state changed to: {lastState}");

                    switch (lastState)
                    {
                        case ChannelState.Idle:
                            GwServerPlugin.Logger.LogInfo(
                                "gRPC Channel is Idle. Forcing connection attempt to wake it up...");
                            _ = channel.ConnectAsync();
                            break;

                        case ChannelState.Connecting:
                            GwServerPlugin.Logger.LogDebug("Channel is attempting to connect...");
                            break;

                        case ChannelState.TransientFailure:
                            GwServerPlugin.Logger.LogWarning(
                                "Connection lost or failed. gRPC will automatically backoff and retry.");
                            break;

                        case ChannelState.Ready:
                            GwServerPlugin.Logger.LogInfo("Channel is Ready! Establishing streams...");
                            Connect(channel);
                            break;
                        case ChannelState.Shutdown:
                            GwServerPlugin.Logger.LogInfo("Channel was explicitly shut down. Stopping monitor.");
                            return;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            GwServerPlugin.Logger.LogInfo("Connection monitoring task was cancelled.");
        }
        catch (Exception ex)
        {
            GwServerPlugin.Logger.LogError($"Critical error in connection monitor: {ex.Message}");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private void Connect(ChannelBase channel)
    {
        Client = new EdgeAgentService.EdgeAgentServiceClient(channel);
        var chatStream = Client.SendChatLogsStream();
        ChatLogStream = chatStream.RequestStream;

        BanInputBehaviour(Client.SubscribeToBans(new Empty()));
        CommandBehaviour(Client.SubscribeToCommands());
        _ = StatusRequestBehaviour(Client.StatusStream(), GwServerPlugin.shutdownCts.Token);
        ProcessDiscordMessages(chatStream.ResponseStream);
    }

    private void CommandBehaviour(AsyncDuplexStreamingCall<CommandResult, Command> stream)
    {
        stream.ResponseStream.ForEachAsync(async data =>
        {
            if (!data.Result)
            {
                _ = CommandService.TryExecuteCommand(data.Name, [.. data.Arguments], data.PermLevel);
                return;
            }

            var result = await CommandService.TryExecuteCommand(data.Name, [.. data.Arguments], data.PermLevel);
            await stream.RequestStream.WriteAsync(new CommandResult
            {
                RequestID = data.RequestID,
                Ok = result.success,
                Result = result.response
            });
        });
    }

    private static void BanInputBehaviour(AsyncServerStreamingCall<BanRequest> stream)
    {
        stream.ResponseStream.ForEachAsync(data =>
        {
            try
            {
                if (data.ShouldBeBanned)
                    PlayerUtils.BanPlayer(data.SteamID, data.Reason, null, false);
                else
                    AllowBanListUtils.UnbanAndRemoveId(
                        Globals.NetworkManagerNuclearOptionInstance.Authenticator.BanList,
                        Globals.DedicatedServerManagerInstance.Config.BanListPaths[0],
                        new CSteamID(data.SteamID));
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        });
    }
    
    
    private static StatusResponse GetCurrentStatus()
    {
        var missionKey = Globals.DedicatedServerManagerInstance.currentMissionOption.Key;
        var name = missionKey.TryGetKey(out var key) ? key.Name : missionKey.Name;
        
        return new StatusResponse
        {
            Ok = true,
            MaxPlayers = (uint)Globals.NetworkManagerNuclearOptionInstance.Server.PeerConfig.MaxConnections,
            PlayerNumber = (uint)PlayerUtils.GetPlayerCount(),
            MissionName = name ?? "Not started",
            MissionStart = DateTime.UtcNow.AddSeconds(-MissionService.CurrentMissionTime).ToTimestamp(),
            LastRestart = DateTime.UtcNow.AddSeconds(-Time.realtimeSinceStartup).ToTimestamp()
        };
    }
    
    private static async Task StatusRequestBehaviour(
        AsyncClientStreamingCall<StatusResponse, Empty> stream,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await stream.RequestStream.WriteAsync(GetCurrentStatus());
            
            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        }
    }
    private static void ProcessDiscordMessages(IAsyncStreamReader<ChatBack> inputStream)
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
}