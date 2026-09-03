using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Features.CommandUtils;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.Voting.Commands;

/// <summary>
///     Command to vote for anything in the dynamic voting system.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class VoteCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand
{
    /// <inheritdoc />
    public override string OutputName => "vote";
    
    /// <inheritdoc />
    public override string Description => "Votes for the currently ongoing vote session";
    
    /// <inheritdoc />
    public override string Usage =>
        $"vote <Outcome>. You can use  \"{PluginConfig.CommandPrefixChar}vote ?\" to get available options";
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
    
    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args)
    {
        if (args.Length < 1) return UniTask.FromResult(false);
        return UniTask.FromResult((args[0] == "?" && args.Length == 1) ||
                                  (VoteManager.Session?.ValidateVote(player, string.Join(" ", args)) ?? false));
    }
    
    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        if (VoteManager.Session == null) return UniTask.FromResult((false, "No vote was started"))!;
        if (args[0] == "?") return UniTask.FromResult((true, string.Join("\n", VoteManager.Session.GetAllOutcomes())))!;
        var rst = VoteManager.Session.TryAddVote(player,  string.Join(" ", args), out var response);
        return UniTask.FromResult((rst, response))!;
    }
}