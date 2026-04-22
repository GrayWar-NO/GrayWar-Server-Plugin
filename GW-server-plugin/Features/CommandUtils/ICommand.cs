using GW_server_plugin.Enums;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils;

/// <summary>
///     Interface for defining commands
/// </summary>
public interface ICommand
{
    /// <summary>
    ///     The command name.
    /// </summary>
    string Name { get; }

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