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
    internal AsyncClientStreamingCall<ChatLog, Ack>? ChatLogsStream;

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
        ChatLogsStream = Client.SendChatLogsStream();
        Client.SubscribeToBans(new Empty());
        
        BanInputBehaviour(Client.SubscribeToBans(new Empty()));
        CommandBehaviour(Client.SubscribeToCommands());

    }

    private void CommandBehaviour(AsyncDuplexStreamingCall<CommandResult, Command> stream)
    {
        stream.ResponseStream.ForEachAsync(async data =>
        {
            if (!data.Result)
            {
                _ = CommandService.TryExecuteCommand(data.Name, data.Arguments.ToArray());
                return;
            } 
            var result = await CommandService.TryExecuteCommand(data.Name, data.Arguments.ToArray());
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
                    ? CommandService.TryExecuteCommand("ban", [data.SteamID.ToString(), data.Reason]) 
                    : CommandService.TryExecuteCommand("unban", [data.SteamID.ToString()]);
                _logSuppressedSteamIDs.Add(data.SteamID);
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        });
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