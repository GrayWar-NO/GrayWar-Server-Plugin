using NuclearOption.ModdingApi;

namespace GW_server_plugin.Features.CommandUtils;

/// <summary>
///     Interface for in-game commands.
/// </summary>
public interface IGameCommand: ICommand
{
    /// <summary>
    ///     Callback to apply the function's behaviour.
    /// </summary>
    ChatCommandResult Execute(ChatCommandRequest request);
}