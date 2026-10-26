using System;
using System.Security;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using GW_server_plugin.Helpers;
using NuclearOption.ModdingApi;
using Steamworks;
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
        new("player (steamID)", ChatArgumentType.Text) // TODO wait for ulong type
    ];
    
    /// <inheritdoc />
    public override UniTask<ChatCommandResult> Execute(string[] args)
    {
        string? response;
        var target = args[0];
        ulong banSteamID;
        if (ulong.TryParse(target, out var targetID) &&
            targetID > (ulong)Globals.DedicatedServerManagerInstance.Config.MaxPlayers)
        {
            banSteamID = targetID;
            response = $"Unbanned player with steamID {banSteamID}";
        }
        else
        {
            var rs = PlayerUtils.TryFindPlayer(target, out var player);
            if (!rs)
                throw new VerificationException(
                    $"Could not find player {target}: validation was not called properly.");
            banSteamID = player!.SteamID;
            response = $"Unbanned player {player.GetDisplayName()}";
        }

        AllowBanListUtils.UnbanAndRemoveId(
            Globals.NetworkManagerNuclearOptionInstance.Authenticator.BanList,
            Globals.DedicatedServerManagerInstance.Config.BanListPaths[0],
            new CSteamID(banSteamID));
        var log = new BanRequest
        {
            SteamID = banSteamID,
            BanEnd = DateTime.UtcNow.ToTimestamp(),
            ShouldBeBanned = false
        };
        GwServerPlugin.GrpcMgr.Client?.SendBanAsync(log);
        return UniTask.FromResult(ChatCommandResult.Ok(response));
    }

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
}
