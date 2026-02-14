using System;
using System.Collections.Generic;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.Commands;

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
    /// <param name="response"> The command response as a string. </param>
    /// <returns></returns>
    public static bool TryExecuteCommand(string commandName, string[] args, Player player, out string? response)
    {
        if (!TryGetCommand(commandName, out var command))
        {
            GwServerPlugin.Logger.LogWarning($"Failed to execute command '{commandName}': Not found.");
            response = $"Did not find a command called '{commandName}'.";
            return false;
        }

        return TryExecuteCommand(command, args, player, out response);
    }
    
    /// <summary>
    ///     Try to execute a command from name.
    ///     This is the IPC implementation, it doesn't require a player
    /// </summary>
    /// <param name="commandName"> The name of the command. </param>
    /// <param name="args"> The arguments for the command. </param>
    /// <param name="response"> The command response as a string. </param>
    /// <returns></returns>
    public static bool TryExecuteCommand(string commandName, string[] args, out string? response)
    {
        if (!TryGetCommand(commandName, out var command))
        {
            GwServerPlugin.Logger.LogWarning($"Failed to execute command '{commandName}': Not found.");
            response = $"Did not find a command called '{commandName}'.";
            return false;
        }

        return TryExecuteCommand(command, args, out response);
    }


    /// <summary>
    ///     Try to execute a command from an <see cref="ICommand"/>.
    /// </summary>
    /// <param name="player"> The player executing the command. </param>
    /// <param name="command"> The command to execute. </param>
    /// <param name="args"> The arguments for the command. </param>
    /// <param name="response"> The command response </param>
    /// <returns></returns>
    public static bool TryExecuteCommand(ICommand command, string[] args, Player player, out string? response)
    {
        if (PermissionLevelUtils.GetPlayerPermissionLevel(player) < command.PermissionLevel)
        {
            GwServerPlugin.Logger.LogWarning($"Player {player.PlayerName} does not have permission to execute command {command.Name}");
            response = $"You are not authorized to execute command {command.Name}";
            return false;
        }

        if (command.Validate(player, args))
        {
            if (command.Execute(player, args, out response))
            {
                GwServerPlugin.Logger.LogInfo(
                    $"Command {command.Name} executed successfully by {player.PlayerName} with argument(s): {string.Join(", ", args)}"
                    );
                return true;
            }

            GwServerPlugin.Logger.LogWarning(
                $"Failed to execute command {command.Name} by {player.PlayerName} with argument(s): {string.Join(", ", args)}");
            response ??= $"Failed to execute command {command.Name}";
            return false;
        }

        GwServerPlugin.Logger.LogWarning($"Failed validation for command {command.Name} by {player.PlayerName} with argument(s): {string.Join(", ", args)}");
        response = $"Invalid arguments: {command.Usage}";
        return false;
    }
    
    /// <summary>
    ///     Try to execute a command from an <see cref="ICommand"/>.
    ///     This is the IPC implementation, it doesn't require a player
    /// </summary>
    /// <param name="command"> The command to execute. </param>
    /// <param name="args"> The arguments for the command. </param>
    /// <param name="response"> The command response </param>
    /// <returns></returns>
    public static bool TryExecuteCommand(ICommand command, string[] args, out string? response)
    {
        PermissionLevelUtils.TryParsePermissionLevel(PluginConfig.IpcCommandPermissionLevel!.Value, out var level);
        if (level < command.PermissionLevel)
        {
            GwServerPlugin.Logger.LogWarning($"The remote process does not have permission to execute command {command.Name}");
            response = $"You are not authorized to execute command {command.Name}";
            return false;
        }

        if (command.Validate(args))
        {
            if (command.Execute(args, out response))
            {
                GwServerPlugin.Logger.LogInfo(
                    $"Command {command.Name} executed successfully by remote process with argument(s): {string.Join(", ", args)}"
                );
                return true;
            }

            GwServerPlugin.Logger.LogWarning(
                $"Failed to execute command {command.Name} by remote process with argument(s): {string.Join(", ", args)}");
            response ??= $"Failed to execute command {command.Name}";
            return false;
        }

        GwServerPlugin.Logger.LogWarning($"Failed validation for command {command.Name} by remote process with argument(s): {string.Join(", ", args)}");
        response = $"Invalid arguments for command {command.Name}\n{command.Usage}";
        return false;
    }

    
}