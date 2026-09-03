using System.Collections.Generic;
using Com.Graywar.NoServerManager.Proto;

namespace GW_server_plugin.Features.CommandUtils;

/// <summary>
///     Interface for defining commands
/// </summary>
public interface ICommand
{
    /// <summary>
    ///     Command names to be used when executing it.
    /// </summary>
    IEnumerable<string> Names { get; }
    
    /// <summary>
    ///     The command name that will be used in config and in /help.
    /// </summary>
    string OutputName { get; }

    /// <summary>
    ///     The command description.
    /// </summary>
    string Description { get; }

    /// <summary>
    ///     The command usage.
    /// </summary>
    string Usage { get; }

    /// <summary>
    ///     The permission level required to execute the command.
    /// </summary>
    public PermissionLevel PermissionLevel { get; }

}