using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Set a rank to a player
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class SetRankCommand(ConfigFile config): ConfigurableCommand(config), IGameCommand, IConsoleCommand
{

    /// <inheritdoc />
    public override string Name => "setrank";

    /// <inheritdoc />
    public override string Description => "Set a rank to a player";

    /// <inheritdoc />
    public override string Usage => "setrank <target / targetID> <rank>";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);
    
    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args) => UniTask.FromResult(args.Length == 2);
    
    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Execute(args);

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
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
        
        return UniTask.FromResult<(bool, string?)>((true, $"You have successfully set rank {rank} to {targetPlayer.GetDisplayName()}."));
        
    }
    
    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}
