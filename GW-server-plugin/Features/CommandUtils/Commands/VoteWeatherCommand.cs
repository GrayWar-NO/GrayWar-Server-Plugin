using System;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using NuclearOption.Networking;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Vote for specific time
/// </summary>
/// <param name="config"></param>
public class VoteWeatherCommand(ConfigFile config) : PermissionConfigurableCommand(config), IGameCommand
{
    public override string Name { get; } = "voteweather";
    public override string Description { get; } = "vote the weather";
    public override string Usage { get; } = $"{PluginConfig.CommandPrefixChar}votetime <clear/rainy/stormy>";
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Everyone;

    public UniTask<bool> Validate(Player player, string[] args)
    {
        if (!GenericVoteService.CanStartVote())
        {
            ChatService.SendPrivateChatMessage("Cannot start a new vote, please wait for current vote to expire.",
                player);
            return UniTask.FromResult<bool>(false);
        }

        if (args.Length != 1)
            return UniTask.FromResult(false);
        if (args[0] == "clear" || args[0] == "rainy" || args[0] == "stormy")
            return UniTask.FromResult(true);

        return UniTask.FromResult(false);
    }

    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
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
        
        GenericVoteService.StartVote(
            player,
            OnPass,
            true,
            false,
            reason: $"Set to {weather}",
            targetName: "Weather"
        );
        return UniTask.FromResult<(bool success, string? response)>((true, ""));
    }
}