using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils;

/// <summary>
///     Manages all command-related stuff that's not handled in vanilla game.
/// </summary>
public static class CommandService
{
    private static readonly HashSet<IConsoleCommand> RegisteredConsoleCommands = [];
    
    /// <summary>
    ///     Registers a command to the Console and to Builtin chat commands, only if available for either.
    /// </summary>
    /// <param name="command"></param>
    public static void RegisterCommand(ICommand command)
    {
        if (command is IConsoleCommand consoleCommand)
            RegisteredConsoleCommands.Add(consoleCommand);
        if (command is IGameCommand gameCommand)
            RegisterBuiltinCommand(gameCommand);
    }
    
    /// <summary>
    ///     Unregisters a command from the places it can be registered.
    /// </summary>
    /// <param name="command"></param>
    public static void UnregisterCommand(ICommand command)
    {
        if (command is IConsoleCommand consoleCommand)
            RegisteredConsoleCommands.Remove(consoleCommand);
        if (command is IGameCommand gameCommand)
            ChatCommands.UnregisterServerCommand(gameCommand.Name);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static IReadOnlyList<IConsoleCommand> GetConsoleCommands() =>
        RegisteredConsoleCommands.ToList().AsReadOnly();
    
    
    private static void RegisterBuiltinCommand(IGameCommand command)
    {
        ChatCommands.RegisterServerCommand(
            command.Name,
            command.Description,
            command.Execute,
            command.Arguments,
            ProtoToShockfrontPermissionLevel(command.PermissionLevel),
            command.Aliases
        );
    }
    
    private static NuclearOption.ModdingApi.PermissionLevel ProtoToShockfrontPermissionLevel(
        PermissionLevel permissionLevel)
    {
        switch (permissionLevel)
        {
            case PermissionLevel.Owner:
                return NuclearOption.ModdingApi.PermissionLevel.Host;
            case PermissionLevel.Admin:
                return NuclearOption.ModdingApi.PermissionLevel.Admin;
            case PermissionLevel.Moderator:
                return NuclearOption.ModdingApi.PermissionLevel.Moderator;
            case PermissionLevel.Everyone:
            default:
                return NuclearOption.ModdingApi.PermissionLevel.Player;
        }
    }
    
    /// <summary>
    ///     Tries to fetch a console command by name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="command"></param>
    /// <returns>true if success</returns>
    public static bool TryGetConsoleCommand(string name, out IConsoleCommand? command)
    {
        try
        {
            command = RegisteredConsoleCommands.First(c => c.MatchesNameOrAlias(name));
            return true;
        }
        catch (InvalidOperationException)
        {
            command = null;
            return false;
        }    
        return false;
    }
    
    /// <summary>
    ///     
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    /// <param name="permissionLevel"></param>
    /// <returns></returns>
    public static UniTask<ChatCommandResult> ExecuteConsoleCommand(string name, string[] args,
        PermissionLevel permissionLevel)
    {
        var command = RegisteredConsoleCommands.FirstOrDefault(c => c.MatchesNameOrAlias(name));
        if (command == null) return UniTask.FromResult(ChatCommandResult.ClientError($"No such command: {name}"));
        if (permissionLevel < command.PermissionLevel)
        {
            GwServerPlugin.Logger.LogWarning(
                $"Remote user with permission level {permissionLevel.ToString()} tried to execute command {command.Name} (level {command.PermissionLevel.ToString()}");
            return UniTask.FromResult(ChatCommandResult.ClientError(
                $"You are not authorized to execute command {command.Name}, as it requires the {command.PermissionLevel.ToString()} permission level."));
        }
        
        return command.Execute(args);
    }
}