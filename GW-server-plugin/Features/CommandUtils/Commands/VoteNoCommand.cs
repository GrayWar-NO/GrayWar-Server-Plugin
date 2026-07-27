using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Vote NO to the current Generic vote session
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class VoteNoCommand(ConfigFile config) : PermissionConfigurableCommand(config), IGameCommand
{
    /// <inheritdoc />
    public override string Name => "n";
    
    /// <inheritdoc />
    public override string Description => "adds a NO vote to the current vote";
    
    /// <inheritdoc />
    public override string Usage => $"n";

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
        GenericVoteService.HandleVote(player, false, out var result);
        return UniTask.FromResult(result);
    }
}