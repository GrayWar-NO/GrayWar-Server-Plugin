using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Set a rank to a player
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class SetRankCommand(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string OutputName => "setrank";
    
    /// <inheritdoc />
    public override string Description => "Set a rank to a player";
    
    /// <inheritdoc />
    public override string Usage => "setrank <target / targetID> <rank>";
    
    /// <inheritdoc />
    public override UniTask<bool> Validate(string[] args) => UniTask.FromResult(args.Length == 2);
    
    /// <inheritdoc />
    public override UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var found = PlayerUtils.TryFindPlayer(args[0], out var targetPlayer);
        if (!found || targetPlayer == null)
            return UniTask.FromResult<(bool, string?)>((false, $"Could not find a player by {args[0]}"));
        
        var rankInput = args[1].Trim();
        
        if (!int.TryParse(rankInput, out var rank))
            return UniTask.FromResult<(bool, string?)>((false, $"Could not parse '{args[1]}' as an integer."));
        
        if (rank is <= 0 or > 6)
            return UniTask.FromResult<(bool, string?)>((false, "Rank must be 0-6"));
        
        targetPlayer.SetRank(rank, false);
        ChatService.SendPrivateChatMessage($"Staff has set your rank to {rank}!", targetPlayer);
        
        return UniTask.FromResult<(bool, string?)>((true,
            $"You have successfully set rank {rank} to {targetPlayer.GetDisplayName()}."));
    }
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}