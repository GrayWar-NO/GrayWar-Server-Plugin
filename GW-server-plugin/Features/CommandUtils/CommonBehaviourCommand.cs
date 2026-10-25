using BepInEx.Configuration;
using NuclearOption.ModdingApi;

namespace GW_server_plugin.Features.CommandUtils;

/// <summary>
///     Type for commands that use the same Validate() and Execute() methods for console and game paths.
/// </summary>
public abstract class CommonBehaviourCommand(ConfigFile config) : ConfigurableCommand(config), IConsoleCommand, IGameCommand
{
    
    /// <inheritdoc />
    public ChatCommandResult Execute(ChatCommandRequest rq) => Execute([.. rq.Args]);
    
    /// <inheritdoc />
    public abstract ChatCommandResult Execute(string[] args);
}