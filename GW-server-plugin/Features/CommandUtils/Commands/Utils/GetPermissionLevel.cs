using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Utils;

/// <summary>
///     Command to get self permission level. Only built in debug.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class GetPermissionLevel(ConfigFile config): ConfigurableCommand(config), IGameCommand
{
    /// <inheritdoc />
    public override string Name => "GetPermissionLevel";
    
    /// <inheritdoc />
    public override string Description => "gets your permission level";
    
    /// <inheritdoc />
    public UniTask<ChatCommandResult> Execute(ChatCommandRequest request)
    {
        return UniTask.FromResult(ChatCommandResult.Ok(request.Sender.PermissionLevel.ToString()));
    }
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}