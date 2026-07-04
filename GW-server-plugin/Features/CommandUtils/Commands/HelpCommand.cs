using System.Linq;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command for getting help
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class HelpCommand(ConfigFile config): PermissionConfigurableCommand(config), IConsoleCommand, IGameCommand
{
    /// <inheritdoc />
    public override string Name { get; } = "help";

    /// <inheritdoc />
    public override string Description { get; } =
        "Get help on other commands, or the list of commands you have available.";

    /// <inheritdoc />
    public override string Usage { get; } = "help [command name]";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(args.Length <= 1);
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        if (args.Length != 0) return Execute(args);
        
        var accessibleCommands = CommandService.GetGameCommands()
            .Where(c => c.PermissionLevel <= PlayerUtils.GetPlayerPermissionLevel(player)).ToList();
        var commandNames = accessibleCommands.Select(c => c.Name).ToList();
        return UniTask.FromResult<(bool, string?)>((true, $"You have access to the following commands: {string.Join(", ", commandNames)}"));
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        if (args.Length == 0)
        {
            var commandNames = CommandService.GetConsoleCommands().Select(c => c.Name).ToList();
            return UniTask.FromResult<(bool, string?)>((true, $"Available commands: {string.Join(", ", commandNames)}"));
        }
        
        var commandName = args[0];
        if (!CommandService.TryGetCommand(commandName, out var command))
        {
            return UniTask.FromResult<(bool, string?)>((false, $"Command {commandName} not found."));
        }

        return UniTask.FromResult<(bool, string?)>((true, $"Command '{command.Name}': {command.Description}\nUsage: {PluginConfig.CommandPrefix!.Value}{command.Usage}"));
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Everyone;
}