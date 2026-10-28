using System;
using System.Linq;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using NuclearOption.Chat;
using NuclearOption.ModdingApi;
using NuclearOption.Networking;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Sends a private message to a specific player.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class WhisperCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string Name => "whisper";
    
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("user", ChatArgumentType.Player),
        new("message", ChatArgumentType.Text)
    ];
    
    /// <inheritdoc />
    public UniTask<ChatCommandResult> Execute(string[] args)
    {
        ChatHelper.TryParseSteamId(args[0], out var steamId);
        if (!UnitRegistry.playerLookupSteamId.TryGetValue(steamId, out var target))
            return UniTask.FromResult(ChatCommandResult.Ok($"Player with steamId {steamId} is not in the server"));
        
        return Common(string.Join(" ", args.Skip(1)), target, null);
    }
    
    /// <inheritdoc />
    public override string Description => "Send a private message to a specified user.";
    
    /// <inheritdoc />
    public UniTask<ChatCommandResult> Execute(ChatCommandRequest request)
    {
        var args = request.Args;
        if (!request.TryGetTargetPlayer(0, out var target, out var error))
        {
            return UniTask.FromResult(ChatCommandResult.ClientError(error));
        }

        var message = string.Join(" ", args.Skip(1));
        
        return Common(message, target.player, request.Sender.player);
    }
    
    private UniTask<ChatCommandResult> Common(string message, Player target, Player? sender)
    {
        message = $"(whisper): {message}";
        ChatService.SendPrivateChatMessage(message, target, sender);
        if (sender != null)
        {
            var senderSteamId = sender.SteamID;
            
            var log = new ChatLog
            {
                Message = message,
                MessageChannel = $"whisper({senderSteamId}:{target.SteamID})",
                MessageSendTime = DateTime.UtcNow.ToTimestamp(),
                SenderSteamID = senderSteamId
            };
            GwServerPlugin.GrpcMgr.ChatLogStream?.WriteAsync(log);
        }        
        return UniTask.FromResult(ChatCommandResult.Ok($"Message sent to {target.MentionArg} successfully."));
    }
    


    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}
