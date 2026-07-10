using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils;

/// <summary>
///     Service for handling commands
/// </summary>
public static class CommandService
{
    private static readonly List<ICommand> Commands = [];
    
    /// <summary> 
    ///     Get all registered commands
    /// </summary>
    /// <returns> All registered commands, Read only.</returns>
    public static IEnumerable<ICommand> GetCommands() => Commands.AsReadOnly();


    /// <summary>
    ///     Get all registered in-game callable commands.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<IGameCommand> GetGameCommands() => Commands.AsReadOnly().OfType<IGameCommand>();
    
    /// <summary>
    ///     Get all registered console commands.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<IConsoleCommand> GetConsoleCommands() => Commands.AsReadOnly().OfType<IConsoleCommand>();
    
    /// <summary>
    ///     Add a command
    /// </summary>
    /// <param name="command"> The command to add. </param>
    public static void AddCommand(ICommand command) => Commands.Add(command);
    
    
    /// <summary>
    ///     Attempt to get a command by name.
    /// </summary>
    /// <param name="commandName"> The name of the command. </param>
    /// <param name="command"> The command, if available. </param>
    /// <returns>true if a command was found, else false</returns>
    public static bool TryGetCommand(string commandName, out ICommand command)
    {
        command = Commands.Find(c => string.Equals(c.Name, commandName, StringComparison.CurrentCultureIgnoreCase));
        return command != null;
    }

    /// <summary>
    ///     Try to execute a command from name.
    /// </summary>
    /// <param name="player"> The player executing the command. </param>
    /// <param name="commandName"> The name of the command. </param>
    /// <param name="args"> The arguments for the command. </param>
    /// <returns></returns>
    public static async UniTask<(bool success, string? response)> TryExecuteCommand(string commandName, string[] args, Player player)
    {
        if (!TryGetCommand(commandName, out var command))
        {
            GwServerPlugin.Logger.LogWarning($"Failed to execute command '{commandName}': Not found.");
            return (false, $"Did not find a command called '{commandName}'."); 
        }
        if (command is not IGameCommand gameCommand)
        {
            return (false, $"{commandName} is not a valid command."); 
        }
        return await TryExecuteCommand(gameCommand, args, player);
    }
    
    /// <summary>
    ///     Try to execute a command from name.
    ///     This is the IPC implementation, it doesn't require a player
    /// </summary>
    /// <param name="commandName"> The name of the command. </param>
    /// <param name="args"> The arguments for the command. </param>
    /// <returns></returns>
    public static async UniTask<(bool success, string? response)> TryExecuteCommand(string commandName, string[] args)
    {
        if (!TryGetCommand(commandName, out var command))
        {
            GwServerPlugin.Logger.LogWarning($"Failed to execute command '{commandName}': Not found.");
            return (false, $"Did not find a command called '{commandName}'."); 
        }
        if (command is not IConsoleCommand consoleCommand)
        {
            return (false, $"{commandName} is not a valid console command."); 
        }
        return await TryExecuteCommand(consoleCommand, args);
    }


    /// <summary>
    ///     Try to execute a command from an <see cref="ICommand"/>.
    /// </summary>
    /// <param name="command"> The command to execute. </param>
    /// <param name="args"> The arguments for the command. </param>
    /// <param name="player"> The player executing the command. </param>
    /// <returns></returns>
    public static async UniTask<(bool success, string? response)> TryExecuteCommand(
        IGameCommand command,
        string[] args,
        Player player)
    {
        if (PermissionLevelUtils.GetPlayerPermissionLevel(player) < command.PermissionLevel)
        {
            GwServerPlugin.Logger.LogWarning($"Player {player.PlayerName} does not have permission to execute command {command.Name}");
            return (false, $"You are not authorized to execute command {command.Name}");
        }
        string? response;
        if (await command.Validate(player, args))
        {
            var executionResult = await command.Execute(player, args);
            response = executionResult.response;
            if (executionResult.success)
            {
                GwServerPlugin.Logger.LogInfo(
                    $"Command {command.Name} executed successfully by {player.PlayerName} with argument(s): {string.Join(", ", args)}"
                );
                return (true, response);
            }

            GwServerPlugin.Logger.LogWarning(
                $"Failed to execute command {command.Name} by {player.PlayerName} with argument(s): {string.Join(", ", args)}");
            response ??= $"Failed to execute command {command.Name}";
            return (false, response);
        }

        GwServerPlugin.Logger.LogInfo(
            $"Failed validation for command {command.Name} by {player.PlayerName} with argument(s): {string.Join(", ", args)}");
        response = $"Invalid arguments: {PluginConfig.CommandPrefix!.Value}{command.Usage}";
        return (false, response);
    }
    
    /// <summary>
    ///     Try to execute a command from an <see cref="ICommand"/>.
    ///     This is the IPC implementation, it doesn't require a player
    /// </summary>
    /// <param name="command"> The command to execute. </param>
    /// <param name="args"> The arguments for the command. </param>
    /// <returns></returns>
    public static async UniTask<(bool success, string? response)> TryExecuteCommand(IConsoleCommand command, string[] args)
    {
        
        PermissionLevelUtils.TryParsePermissionLevel(PluginConfig.ConsoleCommandPermissionLevel!.Value, out var level);
        if (level < command.PermissionLevel)
        {
            GwServerPlugin.Logger.LogWarning($"The remote process does not have permission to execute command {command.Name}");
            return (false, $"You are not authorized to execute command {command.Name}");
        }

        string? response;
        if (await command.Validate(args))
        {
            var executionResult = await command.Execute(args);
            response = executionResult.response;
            if (executionResult.success)
            {
                GwServerPlugin.Logger.LogInfo(
                    $"Command {command.Name} executed successfully by remote process with argument(s): {string.Join(", ", args)}"
                );
                return (true, response);
            }

            GwServerPlugin.Logger.LogWarning(
                $"Failed to execute command {command.Name} by remote process with argument(s): {string.Join(", ", args)}");
            response ??= $"Failed to execute command {command.Name}";
            return (false, response);
        }

        GwServerPlugin.Logger.LogWarning(
            $"Failed validation for command {command.Name} by remote process with argument(s): {string.Join(", ", args)}");
        response = $"Invalid arguments for command {command.Name}\n{PluginConfig.CommandPrefix!.Value}{command.Usage}";
        return (false, response);
    }
}