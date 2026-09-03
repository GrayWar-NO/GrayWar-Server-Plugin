using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Features.CommandUtils;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.Voting.Commands;

/// <summary>
///     Starts a voteSession of the desired type.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class StartVoteCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand
{
    /// <inheritdoc />
    public override IEnumerable<string> DefaultAliases => ["startvote", "votestart", "sv", "vs"];
    
    /// <inheritdoc />
    public override string OutputName => "startvote";
    
    /// <inheritdoc />
    public override string Description => "Starts a vote session";
    
    /// <inheritdoc />
    public override string Usage => $"startvote <{string.Join("/", VoteManager.Factories.Keys)}> <reason>";
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
    
    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) =>
        UniTask.FromResult(args.Length >= 1 && VoteManager.Factories.ContainsKey(args[0]));
    
    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        var session = VoteManager.Factories[args[0]](player, args.Length > 1 ? string.Join(" ", args.Skip(1)) : null);
        var rst = VoteManager.TryStartVote(session, out var response);
        return UniTask.FromResult((rst, response));
    }
}