using System.Linq;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.ModdingApi;
using UnityEngine.Assertions.Must;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Moderation;

/// <summary>
/// Command to kick a player from the server
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class KickCommand(ConfigFile config): ConfigurableCommand(config), IConsoleCommand, IGameCommand
{
    /// <inheritdoc />
    public override string Name => "GwKick";

    /// <inheritdoc />
    public override string Description => "Kicks a player from the  server.";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("player", ChatArgumentType.Player),
        new("reason", ChatArgumentType.Text, true)
    ];

    /// <inheritdoc />
    public async UniTask<ChatCommandResult> Execute(ChatCommandRequest request)
    {
        var args = request.Args;
        if (!request.TryGetTargetPlayer(0, out var target, out var error))
            return ChatCommandResult.ClientError(error);
        if (target != request.Sender) return await Execute([.. args]);
        return ChatCommandResult.ClientError("You cannot kick yourself!");
    }

    /// <inheritdoc />
    public UniTask<ChatCommandResult> Execute(string[] args)
    {
        var target = args[0];
        if (PlayerUtils.TryFindPlayer(target, out var targetPlayer, out var error))
        {
            PlayerUtils.KickPlayer(targetPlayer!.Value.GetInternalPlayer(),  args[1]);
            return UniTask.FromResult(ChatCommandResult.Ok($"{targetPlayer.Value.Mention} has been kicked!"));
        }
        return UniTask.FromResult(ChatCommandResult.ClientError(error));
    }

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
    
}
