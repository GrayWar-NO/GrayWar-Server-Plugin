namespace GW_server_plugin.Features.CommandUtils;

/// <summary>
///     Interface for defining console commands
/// </summary>
public interface IConsoleCommand: ICommand
{
    /// <summary>
    ///     Validate the command arguments for Console use.
    /// </summary>
    /// <param name="args"> The command arguments. </param>
    /// <returns> Whether the command arguments are valid. </returns>
    bool Validate(string[] args);
    
    /// <summary>
    ///     The command action executed by the console.
    /// </summary>
    /// <param name="args"> The command arguments. </param>
    /// <param name="response"> The command response. </param>
    /// <returns> Whether the command was executed successfully. </returns>
    bool Execute(string[] args, out string? response);
}