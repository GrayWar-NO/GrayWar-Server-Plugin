using System.Linq;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
using GW_server_plugin.Features.IPC.Packets;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Sends a private message to a specific player.
/// </summary>
/// <param name="config"></param>
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
            return UniTask.FromResult((false, $"Could not identify a player by \"{args[0]}\"."));
        }

        var message = string.Join(" ", args.Skip(1));
        message = $"(whisper): {message}";
        ChatService.SendPrivateChatMessage(message, target!, sender);
        
        var senderSteamId = sender?.SteamID ?? 0;
        
        var outPacket = new ChatLogPacket
        {
            SteamID = senderSteamId,
            ChatName = $"whisper({senderSteamId}:{target?.SteamID})",
            LogText = message
        };
        GwServerPlugin.LoggingOutBox.Add(outPacket);
        return UniTask.FromResult((true, $"Message sent to {target!.PlayerName}:  {message}"));
    }
    

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}