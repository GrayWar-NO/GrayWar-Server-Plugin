using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.Chat;
using NuclearOption.ModdingApi;
using Steamworks;
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
        if (!ChatCommands.RegisterServerCommand(
                command.Name,
                command.Description,
                command.Execute,
                command.Arguments,
                PermissionLevelUtils.ProtoToShockfrontPermissionLevel(command.PermissionLevel),
                command.Aliases
            ))
            GwServerPlugin.Logger.LogError("Failed to register server command " + command.Name);
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
        catch (Exception)
        {
            command = null;
            return false;
        }
    }
    
    /// <summary>
    ///     
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    /// <param name="permissionLevel"></param>
    /// <returns></returns>
    public static async UniTask<ChatCommandResult> ExecuteConsoleCommand(string name, string[] args,
        PermissionLevel permissionLevel)
    {
        var command = RegisteredConsoleCommands.FirstOrDefault(c => c.MatchesNameOrAlias(name));
        if (command == null) return ChatCommandResult.ClientError($"No such command: {name}");
        if (permissionLevel < command.PermissionLevel)
        {
            GwServerPlugin.Logger.LogWarning(
                $"Remote user with permission level {permissionLevel.ToString()} tried to execute command {command.Name} (level {command.PermissionLevel.ToString()}");
            return ChatCommandResult.ClientError(
                $"You are not authorized to execute command {command.Name}, as it requires the {command.PermissionLevel.ToString()} permission level.");
        }
        
        if (!ValidateConsoleCommandArguments(command, ref args, out var error))
            return ChatCommandResult.ClientError(error);
        TrimNullOrEmpty(ref args);
        
        var result = await command.Execute(args);
        result.Message = ChatUtils.ReplaceMentionsWithNames(result.Message);
        return result;
    }
    
    
    private static void TrimNullOrEmpty(ref string[] array)
    {
        var length = array.Length;
        
        while (length > 0 && string.IsNullOrEmpty(array[length - 1]))
            length--;
        
        if (length < array.Length)
            Array.Resize(ref array, length);
    }
    
    
    private static bool ValidateConsoleCommandArguments(IConsoleCommand command, ref string[] args, out string? error)
    {
        var argumentsToParse = new string[args.Length];
        args.CopyTo(argumentsToParse, 0);
        error = null;
        var currentTextArgIndex = 0;
        args = new string[command.Arguments.Length];
        
        foreach (var (argument, i) in command.Arguments.Select((x, i) => (x, i)))
        {
            if (currentTextArgIndex == argumentsToParse.Length)
            {
                if (argument.Optional)
                    return true;
                error = "Not enough arguments!";
                return false;
            }
            
            var currentArg = argumentsToParse[currentTextArgIndex];
            
            switch (argument.Type)
            {
                case ChatArgumentType.Choices:
                    if (argument.Choices.Any(x => x.Equals(
                            currentArg,
                            StringComparison.OrdinalIgnoreCase)
                        ))
                    {
                        args[i] = argument.Choices.First(x => x.Equals(
                            currentArg,
                            StringComparison.OrdinalIgnoreCase));
                        currentTextArgIndex++;
                    }
                    else if (!argument.Optional)
                    {
                        error = $"Invalid choice for {argument.Name}: {argumentsToParse[currentTextArgIndex]}";
                        return false;
                    }
                    
                    break;
                case ChatArgumentType.Float:
                    if (float.TryParse(currentArg, out _))
                    {
                        args[i] = currentArg;
                        currentTextArgIndex++;
                    }
                    else if (!argument.Optional)
                    {
                        error = $"{currentArg} is not a valid floating point number!";
                        return false;
                    }
                    
                    break;
                
                case ChatArgumentType.Int:
                    if (int.TryParse(currentArg, out _))
                    {
                        args[i] = currentArg;
                        currentTextArgIndex++;
                    }
                    else if (!argument.Optional)
                    {
                        error = $"{currentArg} is not a valid floating point number!";
                        return false;
                    }
                    
                    break;
                case ChatArgumentType.Player:
                    if (!ChatHelper.TryParseSteamId(currentArg, out var steamId))
                    {
                        if (GwServerPlugin.TryGetConnectedPlayerSteamId(currentArg, out var uSteamId))
                            steamId = new CSteamID(uSteamId!.Value);
                        else if (int.TryParse(currentArg, out var id) &&
                            PlayerIdentificationService.TryGetPlayerById(id, out var playerHandle))
                            steamId = playerHandle!.Value.player.CSteamID;
                        else
                        {
                            if (argument.Optional)
                                break;
                            error = $"{currentArg} is not a valid player!";
                            return false;
                        }
                    }
                    
                    args[i] = ChatHelper.FormatMention(steamId.m_SteamID);
                    currentTextArgIndex++;
                    break;
                case ChatArgumentType.Text:
                    if (i == command.Arguments.Length - 1)
                    {
                        args[i] += string.Join(" ", argumentsToParse.Skip(currentTextArgIndex));
                        return true;
                    }
                    
                    args[i] = currentArg;
                    currentTextArgIndex++;
                    break;
                default:
                case ChatArgumentType.Command:
                    args[i] = currentArg;
                    currentTextArgIndex++;
                    break;
            }
        }
        
        return true;
    }
}