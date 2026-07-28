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
public class VoteWeatherCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand
{
    /// <inheritdoc />
    public override string Name => "voteweather";
    
    /// <inheritdoc />
    public override string Description => "vote the weather";
    
    /// <inheritdoc />
    public override string Usage => $"{PluginConfig.CommandPrefixChar}votetime <clear/rainy/stormy>";
    
    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args)
    {
        if (args.Length != 1)
            return UniTask.FromResult(false);
        if (args[0] == "clear" || args[0] == "rainy" || args[0] == "stormy")
            return UniTask.FromResult(true);

        return UniTask.FromResult(false);
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        if (!VoteSession.CanStartVote())
        {
            var response = "Cannot start a new vote, please wait for current vote to expire.";
            return UniTask.FromResult<(bool success, string? response)>((false,response));
        }
        
        var weather = args[0];

        void OnPass()
        {
            if (weather == "clear")
                LevelInfo.i.Networkconditions = (float)0.0;
            else if (weather == "rainy")
                LevelInfo.i.Networkconditions = (float)0.6;
            else if (weather == "stormy")
                LevelInfo.i.Networkconditions = (float)1.0;
        }

        var startingMessage = $"vote to set weather to {weather} has started";
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
            reason: $"Set to {weather}",
            targetName: "Weather"
        );
        return UniTask.FromResult<(bool success, string? response)>((true, null));
    }
}