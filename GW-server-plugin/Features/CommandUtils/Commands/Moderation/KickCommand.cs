using System.Linq;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.ModdingApi;
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
        var player = request.Sender.player;
        var target = args[0];
        PlayerUtils.TryFindPlayer(target, out var targetPlayer);
        if (targetPlayer != player) return await Execute([.. args]);
        return ChatCommandResult.ClientError("You cannot kick yourself!");
    }

    /// <inheritdoc />
    public UniTask<ChatCommandResult> Execute(string[] args)
    {
        var target = args[0];
        if (PlayerUtils.TryFindPlayer(target, out var targetPlayer))
        {
            PlayerUtils.KickPlayer(targetPlayer!,  string.Join(" ", args.Skip(1)));
            return UniTask.FromResult(ChatCommandResult.Ok($"{targetPlayer!.MentionArg} has been kicked!"));
        }

        return UniTask.FromResult(ChatCommandResult.ClientError($"{target} is not a valid player!"));
    }

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
    
}
