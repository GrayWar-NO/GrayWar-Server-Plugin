using System.Linq;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using NuclearOption.ModdingApi;

namespace GW_server_plugin.Features.CommandUtils;

/// <summary>
///     Type for commands that use the same Validate() and Execute() methods for console and game paths.
/// </summary>
public abstract class CommonBehaviourCommand(ConfigFile config)
    : ConfigurableCommand(config), IConsoleCommand, IGameCommand
{
    /// <inheritdoc />
    public async UniTask<ChatCommandResult> Execute(ChatCommandRequest rq)
    {
        return TrySanitizeGameForCommonExec(rq, out var args, out var error)
            ? await Execute(args)
            : ChatCommandResult.ClientError(error);
    }
    
    private bool TrySanitizeGameForCommonExec(ChatCommandRequest rq, out string[] args, out string error)
    {
        error = "";
        args = new string[rq.Args.Count];
        if (rq.Args.Count > Arguments.Length)
        {
            error =
                $"Invalid argument count. Command {Name} requires at most {Arguments.Length} arguments, but {rq.Args.Count} were given.";
            return false;
        }
        
        foreach (var (arg, i) in rq.Args.Select((x, i) => (x, i)))
        {
            if (Arguments[i].Type == ChatArgumentType.Player && rq.TryGetTargetSteamId(i, out var steamId, out _))
            {
                args[i] = steamId.ToString();
            }
            else
                args[i] = arg;
        }
        
        return true;
    }
    
    
    /// <inheritdoc />
    public abstract UniTask<ChatCommandResult> Execute(string[] args);
}