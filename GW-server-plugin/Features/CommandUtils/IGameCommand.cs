using Cysharp.Threading.Tasks;
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
    UniTask<bool> Validate(Player player, string[] args);
    
    /// <summary>
    ///     The command action executed by a player.
    /// </summary>
    /// <param name="player"> The player executing the command. </param>
    /// <param name="args"> The command arguments. </param>
    /// <returns> Whether the command was executed successfully, as well as the command response string </returns>
    UniTask<(bool success, string? response)> Execute(Player player, string[] args);
}