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
[AutoCommand]
public class VoteTimeCommand(ConfigFile config) : PermissionConfigurableCommand(config), IGameCommand
{
    public override string Name { get; } = "votetime";
    public override string Description { get; } = "vote the time";

    public override string Usage { get; } =
        $"{PluginConfig.CommandPrefixChar}votetime <24hr format 0-24> (e.g '{PluginConfig.CommandPrefixChar}votetime 18' for 18:00)";

    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Everyone;

    public UniTask<bool> Validate(Player player, string[] args)
    {
        if (!GenericVoteService.CanStartVote())
        {
            ChatService.SendPrivateChatMessage("Cannot start a new vote, please wait for current vote to expire.",
                player);
            return UniTask.FromResult(false);
        }

        if (args.Length != 1)
            return UniTask.FromResult(false);
        if ((args.Length == 1 && !int.TryParse(args[0], out _)) || (args.Length == 1 && int.Parse(args[0]) < 0) ||
            (args.Length == 1 && int.Parse(args[0]) > 24))
        {
            ChatService.SendPrivateChatMessage("Number invalid. Please Try again.", player);
            return UniTask.FromResult(false);
        }

        return UniTask.FromResult(true);
    }

    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
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
        
        GenericVoteService.StartVote(
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