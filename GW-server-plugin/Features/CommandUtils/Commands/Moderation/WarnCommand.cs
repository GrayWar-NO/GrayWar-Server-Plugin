using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Moderation;

/// <summary>
/// Command to ban a player.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class WarnCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string Name => "warn";
    
    /// <inheritdoc />
    public override string Description => "Warns a player.";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("player", ChatArgumentType.Player),
        new("reason", ChatArgumentType.Text)
    ];
    
    /// <inheritdoc />
    public async UniTask<ChatCommandResult> Execute(ChatCommandRequest request)
    {
        if (!PlayerUtils.TryFindPlayer(request.Args[0], out var targetPlayer, out var error))
            return ChatCommandResult.ClientError(error);
        if (targetPlayer != request.Sender) return await Execute([.. request.Args]);
        return ChatCommandResult.ClientError("You cannot warn yourself!");
    }
    
    /// <inheritdoc />
    public UniTask<ChatCommandResult> Execute(string[] args)
    {
        var reason = args[1];
        
        if (!PlayerUtils.TryFindPlayer(args[0], out var targetPlayer, out var error))
            return UniTask.FromResult(ChatCommandResult.ClientError(error));

        return UniTask.FromResult(
            GwServerPlugin.WarnService.AddWarn(targetPlayer!.Value.SteamId, reason)
            ? ChatCommandResult.Ok($"Warned {targetPlayer.Value.Mention}.")
            : ChatCommandResult.ClientError($"failed to warn {args[0]}: is no longer on server.")
            );
    }
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}