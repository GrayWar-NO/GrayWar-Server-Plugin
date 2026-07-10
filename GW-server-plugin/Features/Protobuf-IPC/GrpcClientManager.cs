using System;
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

    internal EdgeAgentService.EdgeAgentServiceClient Client = null!;
    internal AsyncClientStreamingCall<ChatLog, Ack> ChatLogsStream = null!;
    private AsyncDuplexStreamingCall<CommandResult, Command> _commandStream = null!;
    private AsyncServerStreamingCall<BanRequest> _bansStream = null!;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="config"></param>
    public GrpcClientManager(ConfigFile config)
    {
        _serverName = config.Bind("Manager Interface", "server name", "graywar",
            "Name the server will report to the manager");
        _centralHost = config.Bind("Manager Interface", "central hostname", "graywar.no",
            "Hostname or IP of the manager");
        _centralPort = config.Bind("Manager Interface", "central port", 50051u,
            new ConfigDescription("Port of the manager", new AcceptableValueRange<uint>(0, 65535)));
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
        _bansStream = Client.SubscribeToBans(new Empty());
        _commandStream = Client.SubscribeToCommands();
        
        BanInputBehaviour();
        CommandBehaviour();

    }

    private void CommandBehaviour()
    {
        _commandStream.ResponseStream.ForEachAsync(async data =>
        {
            if (!data.Result)
            {
                _ = CommandService.TryExecuteCommand(data.Name, data.Arguments.ToArray());
                return;
            } 
            var result = await CommandService.TryExecuteCommand(data.Name, data.Arguments.ToArray());
            await _commandStream.RequestStream.WriteAsync(new CommandResult
            {
                RequestID = data.RequestID,
                Ok = result.success,
                Result = result.response
            });

        });
    }

    private void BanInputBehaviour()
    {
        _bansStream.ResponseStream.ForEachAsync(data =>
        {
            try
            {
                _ = data.ShouldBeBanned
                    ? CommandService.TryExecuteCommand("ban", [data.SteamID.ToString(), data.Reason]) 
                    // TODO not get bans logged back.
                    : CommandService.TryExecuteCommand("unban", [data.SteamID.ToString()]);
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        });
    }
}