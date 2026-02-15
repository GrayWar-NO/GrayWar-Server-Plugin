using System.Linq;
using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command for getting help
/// </summary>
/// <param name="config"></param>
public class HelpCommand(ConfigFile config): PermissionConfigurableCommand(config)
{
    /// <inheritdoc />
    public override string Name { get; } = "help";

    /// <inheritdoc />
    public override string Description { get; } =
        "Get help on other commands, or the list of commands you have available.";

    /// <inheritdoc />
    public override string Usage { get; } = "help [command name]";

    /// <inheritdoc />
    public override bool Validate(Player player, string[] args)
    {
        return Validate(args);
    }

    /// <inheritdoc />
    public override bool Validate(string[] args)
    {
        return args.Length <= 1;
    }

    /// <inheritdoc />
    public override bool Execute(Player player, string[] args, out string? response)
    {
        if (args.Length == 0)
        {
            var accessibleCmmands = CommandService.GetCommands()
                .Where(c => c.PermissionLevel <= PlayerUtils.GetPlayerPermissionLevel(player)).ToList();
            var commandNames = accessibleCmmands.Select(c => c.Name).ToList();
            response = $"You have access to the following commands: {string.Join(", ", commandNames)}";
            return true;
        }

        return Execute(args, out response);
    }

    /// <inheritdoc />
    public override bool Execute(string[] args, out string? response)
    {
        if (args.Length == 0)
        {
            var commandNames = CommandService.GetCommands().Select(c => c.Name).ToList();
            response = $"Available commands: {string.Join(", ", commandNames)}";
            return true;
        }
        
        var commandName = args[0];
        if (!CommandService.TryGetCommand(commandName, out var command))
        {
            response = $"Command {commandName} not found.";
            return true;
        }

        response = $"Command '{command.Name}': {command.Description}\nUsage: {command.Usage}";
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Everyone;
}