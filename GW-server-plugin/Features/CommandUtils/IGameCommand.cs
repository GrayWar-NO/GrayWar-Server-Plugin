using GW_server_plugin.Enums;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils;

/// <summary>
///     Interface for in-game commands.
/// </summary>
public interface IGameCommand: ICommand
{
    /// <summary>
    ///     Validate the command arguments.
    /// </summary>
    /// <param name="player"> The player executing the command. </param>
    /// <param name="args"> The command arguments. </param>
    /// <returns> Whether the command arguments are valid. </returns>
    bool Validate(Player player, string[] args);
    
    /// <summary>
    ///     The command action executed by a player.
    /// </summary>
    /// <param name="player"> The player executing the command. </param>
    /// <param name="args"> The command arguments. </param>
    /// <param name="response"> The command response. </param>
    /// <returns> Whether the command was executed successfully. </returns>
    bool Execute(Player player, string[] args, out string? response);
}