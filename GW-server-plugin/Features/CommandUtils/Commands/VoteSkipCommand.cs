using System;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Vote to skip current mission
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class VoteSkipCommand(ConfigFile config) : PermissionConfigurableCommand(config), IGameCommand
{
    /// <inheritdoc />
    public override string Name => "voteskip";

    /// <inheritdoc />
    public override string Description => "Let you skip this mission by voting";

    /// <inheritdoc />
    public override string Usage => "voteskip to initiate a vote to skip the current mission";

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args)
    {
        return UniTask.FromResult(true);
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        if (!VoteSession.CanStartVote())
        {
            var response = "Cannot start a new vote, please wait for current vote to expire.";
            return UniTask.FromResult<(bool success, string? response)>((false, response));
        }

        void OnPass()
        {
            var log = new ChatLog
            {
                MessageChannel = "all",
                MessageSendTime = DateTime.UtcNow.ToTimestamp(),
                Message = "Voteskip has passed",
                SenderSteamID = player.SteamID
            };
            GwServerPlugin.GrpcMgr.ChatLogStream?.WriteAsync(log);

            _ = MissionService.StartNextMission();
        }

        var startingMessage = "A vote to skip the current mission has been started";
        ChatService.SendChatMessageAsServer(startingMessage);

        VoteSession.StartVoteSession(
            player,
            OnPass,
            true,
            false,
            "Voteskip current mission"
        );
        return UniTask.FromResult<(bool success, string? response)>((true, null));
    }
}