using System;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using GW_server_plugin.Features;
using GW_server_plugin.Features.CommandUtils;
using NuclearOption.Networking;

namespace GW_server_plugin.CritzOS;

/// <summary>
/// Votes to queue the next mission
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class VoteMissionCommand(ConfigFile config) : PermissionConfigurableCommand(config), IGameCommand
{
    /// <inheritdoc />
    public override string Name => "votemission";
    /// <inheritdoc />
    public override string Description => "lets you vote to queue the next mission";
    /// <inheritdoc />
    public override string Usage => $"votemission <number> to get list of missions. Use {PluginConfig.CommandPrefixChar}missions to get the list of missions";
    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args)
    {
        if (args.Length > 1)
            return UniTask.FromResult(false);
        if ((args.Length == 1 && !int.TryParse(args[0], out _)) || (args.Length == 1 && int.Parse(args[0]) < 0))
            return UniTask.FromResult(false);
        if (args.Length == 0)
            return UniTask.FromResult(false);
        return UniTask.FromResult(true);
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        var fetchedMissions = MissionService.GetAllAvailableMissionOptions();
        var idx = int.Parse(args[0]);

        if (idx > fetchedMissions.Length - 1 || idx < 1)
        {
            var response = "Mission ID is out of the range of valid numbers. Please try again";
            return UniTask.FromResult<(bool success, string? response)>((false, response));
        }

        void OnPass()
        {
            var m = $"Mission vote for {fetchedMissions![idx].Key.Name} has passed";
            var log = new ChatLog
            {
                MessageChannel = "all",
                MessageSendTime = DateTime.UtcNow.ToTimestamp(),
                Message = $"{m}",
                SenderSteamID = player.SteamID
            };
            GwServerPlugin.GrpcMgr.ChatLogStream?.WriteAsync(log);

            _ = MissionService.SetNextMission(fetchedMissions![idx]);
            fetchedMissions = null;
        }

        if (VoteSession.CanStartVote())
        {
            var startingMessage = $"vote to queue {fetchedMissions[idx].Key.Name} as the next mission has started";
            var log = new ChatLog
            {
                MessageChannel = "all",
                MessageSendTime = DateTime.UtcNow.ToTimestamp(),
                Message = $"{startingMessage}",
                SenderSteamID = player.SteamID
            };
            GwServerPlugin.GrpcMgr.ChatLogStream?.WriteAsync(log);
            
            ChatService.SendChatMessageAsServer(startingMessage);
            VoteSession.StartVoteSession(
                player,
                OnPass,
                false,
                false,
                reason: "Queue as next mission",
                targetName: fetchedMissions[idx].Key.Name
            );
        }
        else
        {
            var response = "Cannot start a new mission vote, please wait for current vote to expire";
            return UniTask.FromResult<(bool success, string? response)>((false, response));
        }
        return UniTask.FromResult<(bool success, string? response)>((true, null));
    }
}