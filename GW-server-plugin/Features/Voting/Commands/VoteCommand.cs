using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Features.CommandUtils;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.Voting.Commands;

/// <summary>
///     Command to vote for anything in the dynamic voting system.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
[DynamicCommand]
public class VoteCommand(ConfigFile config)
    : ConfigurableCommand(config), IGameCommand
{
    /// <inheritdoc />
    public override string Name => "vote";
    
    /// <inheritdoc />
    public override string Description => "Votes for the currently ongoing vote session\n use /vote ? to get more info on outcomes.";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("outcome", ChatArgumentType.Text)
    ];
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
    
    /// <inheritdoc />
    public UniTask<ChatCommandResult> Execute(ChatCommandRequest request)
    {
        var args = request.Args;
        var player = request.Sender.player;
        
        if (args.Count != 1)
            return UniTask.FromResult(ChatCommandResult.ClientError("Invalid number of arguments"));
        if (VoteManager.Session == null)
            return UniTask.FromResult(ChatCommandResult.ClientError("No vote was started"));
        if (args[0] == "?")
            return UniTask.FromResult(ChatCommandResult.Ok(string.Join("\n", VoteManager.Session.GetAllOutcomes())));
        if (!VoteManager.Session.ValidateVote(player, string.Join(" ", args)))
            return UniTask.FromResult(ChatCommandResult.ClientError($"Invalid vote outcome: {args[0]}. Use one of {string.Join("\n", VoteManager.Session.GetAllOutcomes())}"));
        var rst = VoteManager.Session.TryAddVote(player, string.Join(" ", args), out var response);
        return UniTask.FromResult(rst ? ChatCommandResult.Ok(response) : ChatCommandResult.ClientError(response));
    }
}