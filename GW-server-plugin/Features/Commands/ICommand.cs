using GW_server_plugin.Enums;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.Commands;

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
    
    /// <summary>
    ///     Validate the command arguments.
    /// </summary>
    /// <param name="player"> The player executing the command. </param>
    /// <param name="args"> The command arguments. </param>
    /// <returns> Whether the command arguments are valid. </returns>
    bool Validate(Player player, string[] args);

    /// <summary>
    ///     Validate the command arguments.
    /// </summary>
    /// <param name="args"> The command arguments. </param>
    /// <returns> Whether the command arguments are valid. </returns>
    bool Validate(string[] args);

    /// <summary>
    ///     The command action executed by a plyer.
    /// </summary>
    /// <param name="player"> The player executing the command. </param>
    /// <param name="args"> The command arguments. </param>
    /// <param name="response"> The command response. </param>
    /// <returns> Whether the command was executed successfully. </returns>
    bool Execute(Player player, string[] args, out string? response);

    /// <summary>
    ///     The command action executed by the console.
    /// </summary>
    /// <param name="args"> The command arguments. </param>
    /// <param name="response"> The command response. </param>
    /// <returns> Whether the command was executed successfully. </returns>
    bool Execute(string[] args, out string? response);
}