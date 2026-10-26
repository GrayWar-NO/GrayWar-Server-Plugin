using Cysharp.Threading.Tasks;
using NuclearOption.ModdingApi;

namespace GW_server_plugin.Features.CommandUtils;

/// <summary>
/// Async interface for defining console commands
/// </summary>
public interface IConsoleCommand: ICommand
{
    /// <summary>
    ///     The command action executed by the console.
    /// </summary>
    /// <param name="args"> The command arguments. </param>
    /// <returns> Whether the command was executed successfully, aswell as the command response string. </returns>
    ChatCommandResult Execute(string[] args);
}