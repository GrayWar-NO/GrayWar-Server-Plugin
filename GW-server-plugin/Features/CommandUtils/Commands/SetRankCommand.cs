using System.Linq;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Set a rank to a player
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class SetRankCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand
{
    /// <inheritdoc />
    public override string Name => "setrank";
    
    /// <inheritdoc />
    public override string Description => "Set a rank to a player";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("player", ChatArgumentType.Player),
        ChatCommandArgument.FromChoices("rank", [.. Enumerable.Range(0, 7).Select(n => n.ToString())])
    ];
    
    /// <inheritdoc />
    public UniTask<ChatCommandResult> Execute(ChatCommandRequest request)
    {
        var args = request.Args;
        
        if (!request.TryGetTargetPlayer(0, out var target, out var error))
        {
            return UniTask.FromResult(ChatCommandResult.ClientError(error));
        }
        
        var rankInput = args[1].Trim();
        
        if (!int.TryParse(rankInput, out var rank) || rank is <= 0 or > 6)
            return UniTask.FromResult(ChatCommandResult.ClientError($"Invalid rank {rankInput}."));
        
        target.player.SetRank(rank, false);
        ChatService.SendPrivateChatMessage($"Staff has set your rank to {rank}!", target.player);
        
        return UniTask.FromResult(ChatCommandResult.Ok($"Successfully set {target.Mention}'s rank to {rank}!"));
    }
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}