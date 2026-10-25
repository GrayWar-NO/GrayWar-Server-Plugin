using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils;

/// <summary>
///     Interface for defining commands
/// </summary>
public interface ICommand
{
    /// <summary>
    ///     The command name that will be used in config and in /help.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    ///     The command description.
    /// </summary>
    string Description { get; }
    
    /// <summary>
    ///     Arguments to be used when calling the command.
    /// </summary>
    ChatCommandArgument[] Arguments { get; }
    
    
    /// <summary>
    ///     The permission level required to execute the command.
    /// </summary>
    public PermissionLevel PermissionLevel { get; }
    
    /// <summary>
    ///     Aliases the command can be called with.
    /// </summary>
    string[] Aliases { get; }

}