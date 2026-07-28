using System;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Vote for specific time
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class VoteTimeCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand
{
    /// <inheritdoc />
    public override string Name => "votetime";
    
    /// <inheritdoc />
    public override string Description => "vote the time of day";
    
    /// <inheritdoc />
    public override string Usage =>
        $"votetime <24hr format 0-24> (e.g '{PluginConfig.CommandPrefixChar}votetime 18' for 18:00)";
    
    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args)
    {
        if (args.Length != 1)
            return UniTask.FromResult(false);
        if ((args.Length == 1 && !int.TryParse(args[0], out _)) || (args.Length == 1 && int.Parse(args[0]) < 0) ||
            (args.Length == 1 && int.Parse(args[0]) > 24))
            return UniTask.FromResult(false);

        return UniTask.FromResult(true);
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        if (!VoteSession.CanStartVote())
        {
            var response = "Cannot start a new vote, please wait for current vote to expire.";
            return UniTask.FromResult<(bool success, string? response)>((false,response));
        }

        var timeOfDay = int.Parse(args[0]);

        void OnPass()
        {
            LevelInfo.i.NetworktimeOfDay = timeOfDay;
        }

        var startingMessage = $"vote to set time to {timeOfDay}:00 has started";
        var log = new ChatLog
        {
            MessageChannel = "all",
            MessageSendTime = DateTime.UtcNow.ToTimestamp(),
            Message = startingMessage,
            SenderSteamID = player.SteamID
        };
        GwServerPlugin.GrpcMgr.ChatLogStream?.WriteAsync(log);
        ChatService.SendChatMessageAsServer(startingMessage);
        
        VoteSession.StartVoteSession(
            player,
            OnPass,
            true,
            false,
            reason: "Set to {timeOfDay}:00",
            targetName: "Time"
        );
        return UniTask.FromResult<(bool success, string? response)>((true, null));
    }
}