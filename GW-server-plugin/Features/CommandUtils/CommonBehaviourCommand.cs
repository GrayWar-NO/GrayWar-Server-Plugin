using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils;

/// <summary>
///     Type for commands that use the same Validate() and Execute() methods for console and game paths.
/// </summary>
public abstract class CommonBehaviourCommand(ConfigFile config) : ConfigurableCommand(config), IConsoleCommand, IGameCommand
{
    /// <inheritdoc />
    public async UniTask<bool> Validate(Player player, string[] args) => await Validate(args);
    
    /// <inheritdoc />
    public async UniTask<(bool success, string? response)> Execute(Player player, string[] args) => await Execute(args);
    
    /// <inheritdoc />
    public abstract UniTask<(bool success, string? response)> Execute(string[] args);
    
    /// <inheritdoc />
    public abstract UniTask<bool> Validate(string[] args);
}