using System.Linq;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Features.CommandUtils;
using NuclearOption.ModdingApi;
using NuclearOption.Networking;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.Voting.Commands;

/// <summary>
///     Starts a voteSession of the desired type.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class StartVoteCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand // TODO dynamic register/unregister.
{
    /// <inheritdoc />
    protected override string[] DefaultAliases => ["startvote", "votestart", "sv", "vs"];
    
    /// <inheritdoc />
    public override string Name => "startvote";
    
    /// <inheritdoc />
    public override string Description => "Starts a vote session";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        ChatCommandArgument.FromChoices("type", [.. VoteManager.Factories.Keys]),
        new("reason", ChatArgumentType.Text, true)
    ];
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
    
    /// <inheritdoc />
    public ChatCommandResult Execute(ChatCommandRequest request)
    {
        var args = request.Args;
        var player = request.Sender.player;
        var session = VoteManager.Factories[args[0]](player, args[1]);
        if (VoteManager.TryStartVote(session, out var response))
            return ChatCommandResult.Ok(response);
        return ChatCommandResult.ClientError(response);
        
    }
}