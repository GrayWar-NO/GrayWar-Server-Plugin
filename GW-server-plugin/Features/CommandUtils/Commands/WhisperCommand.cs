using System;
using System.Linq;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Sends a private message to a specific player.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class WhisperCommand(ConfigFile config) : PermissionConfigurableCommand(config), IConsoleCommand, IGameCommand
{
    /// <inheritdoc />
    public override string Name => "whisper";

    /// <inheritdoc />
    public override string Description => "Send a private message to a specified user.";

    /// <inheritdoc />
    public override string Usage => "whisper <user Name/ID> <message>";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(args.Length > 1);
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Behaviour(player, args);

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args) => Behaviour(null, args);


    private static UniTask<(bool success, string? response)> Behaviour(Player? sender, string[] args)
    {
        var found = PlayerUtils.TryFindPlayer(args[0], out var target);
        if (!found || !target)
        {
            return UniTask.FromResult<(bool, string?)>((false, $"Could not identify a player by \"{args[0]}\"."));
        }

        var message = string.Join(" ", args.Skip(1));
        message = $"(whisper): {message}";
        ChatService.SendPrivateChatMessage(message, target!, sender);

        var senderSteamId = sender?.SteamID ?? 0;

        var log = new ChatLog
        {
            Message = message,
            MessageChannel = $"whisper({senderSteamId}:{target?.SteamID})",
            MessageSendTime = DateTime.UtcNow.ToTimestamp(),
            SenderSteamID = senderSteamId
        };
        GwServerPlugin.GrpcMgr.ChatLogStream?.WriteAsync(log);

        return UniTask.FromResult<(bool, string?)>((true, $"Message sent to {target!.PlayerName}:  {message}"));
    }


    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}