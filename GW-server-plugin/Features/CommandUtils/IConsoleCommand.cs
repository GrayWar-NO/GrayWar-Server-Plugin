using Cysharp.Threading.Tasks;

namespace GW_server_plugin.Features.CommandUtils;

/// <summary>
/// Async interface for defining console commands
/// </summary>
public interface IConsoleCommand: ICommand
{
    /// <summary>
    ///     Validate the command arguments for Console use.
    /// </summary>
    /// <param name="args"> The command arguments. </param>
    /// <returns> Whether the command arguments are valid. </returns>
    UniTask<bool> Validate(string[] args);
    
    /// <summary>
    ///     The command action executed by the console.
    /// </summary>
    /// <param name="args"> The command arguments. </param>
    /// <returns> Whether the command was executed successfully, aswell as the command response string. </returns>
    UniTask<(bool success, string? response)> Execute(string[] args);
}