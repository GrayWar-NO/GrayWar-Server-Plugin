using System.Linq;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command for getting help
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class HelpCommand(ConfigFile config) : ConfigurableCommand(config), IConsoleCommand
{
    /// <inheritdoc />
    protected override string[] DefaultAliases => ["help", "h", "?"];
    
    /// <inheritdoc />
    public override string Name => "help";
    
    /// <inheritdoc />
    public override string Description =>
        "Get help on other commands, or the list of commands you have available.";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("command", ChatArgumentType.Command, true)
    ];
    
    /// <inheritdoc />
    public UniTask<ChatCommandResult> Execute(string[] args)
    {
        if (args.Length == 0)
        {
            var commandNames = CommandService.GetConsoleCommands().Select(c => c.Name).ToList();
            return UniTask.FromResult(ChatCommandResult.Ok($"Available commands: {string.Join(", ", commandNames)}"));
        }
        
        var commandName = args[0];
        if (!CommandService.TryGetConsoleCommand(commandName, out var command))
        {
            return UniTask.FromResult(ChatCommandResult.ClientError($"Command {commandName} not found."));
        }
        
        var response = $"Command '{command!.Name}': {command.Description}\nArguments";
        
        foreach (var arg in command.Arguments)
        {
            response += $"{arg.Name}: {arg.Type}";
            if (arg.Optional) response += ", optional";
            response += ";\t";
        }
        
        return UniTask.FromResult(ChatCommandResult.Ok(response));
    }
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}