using System;
using System.IO;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Grpc.Core;

namespace GW_server_plugin.Features.Protobuf_IPC;

/// <summary>
/// Manages the embedded plugin GRPC client for the graywar NOServerManager 
/// </summary>
public class GrpcClientManager
{
    private static ConfigEntry<string> _serverName = null!;
    private static ConfigEntry<string> _centralHost = null!;
    private static ConfigEntry<uint> _centralPort = null!;

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
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public EdgeAgentService.EdgeAgentServiceClient InitializeGrpc()
    {
        ChannelCredentials creds = new SslCredentials(
            File.ReadAllText("CA/ca.crt"),
            new KeyCertificatePair(
                File.ReadAllText($"CA/{_serverName.Value}.crt"),
                File.ReadAllText($"CA/{_serverName.Value}.key")
            )
        );
        var channel = new Channel(_centralHost.Value, Convert.ToInt32(_centralPort.Value), creds);
        return new EdgeAgentService.EdgeAgentServiceClient(channel);
    }
}