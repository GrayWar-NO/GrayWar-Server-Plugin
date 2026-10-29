using System;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using GW_server_plugin.Helpers;
using NuclearOption.Chat;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Moderation;

/// <summary>
/// Command to unban a player.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class UnbanCommand(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string Name => "unban";
    
    /// <inheritdoc />
    public override string Description => "Unbans a player from the server.";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("player", ChatArgumentType.Player)
    ];
    
    /// <inheritdoc />
    public override UniTask<ChatCommandResult> Execute(string[] args)
    {
        if (!ChatHelper.TryParseSteamId(args[0], out var banSteamID))
            return UniTask.FromResult(ChatCommandResult.ClientError($"Invalid SteamID: {args[0]}"));
        
        var response = $"Unbanned player with steamID {banSteamID}";
        
        AllowBanListUtils.UnbanAndRemoveId(
            Globals.NetworkManagerNuclearOptionInstance.Authenticator.BanList,
            Globals.DedicatedServerManagerInstance.Config.BanListPaths[0],
            banSteamID);
        var log = new BanRequest
        {
            SteamID = banSteamID.m_SteamID,
            BanEnd = DateTime.UtcNow.ToTimestamp(),
            ShouldBeBanned = false
        };
        GwServerPlugin.GrpcMgr.Client?.SendBanAsync(log);
        return UniTask.FromResult(ChatCommandResult.Ok(response));
    }
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
}