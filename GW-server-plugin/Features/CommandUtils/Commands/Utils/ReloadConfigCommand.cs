using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Utils;

/// <summary>
/// Command for reloading config files.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class ReloadConfigCommand(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string Name => "reload";
    
    /// <inheritdoc />
    public override string Description => "Reload the plugin config, the dedicated server config, or both.";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        ChatCommandArgument.FromChoices("config", ["bepinex", "server", "both"])
    ];
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Admin;
    
    /// <inheritdoc />
    public override UniTask<ChatCommandResult> Execute(string[] args)
    {
        string? response;
        switch (args[0].ToLowerInvariant())
        {
            case "bepinex":
                GwServerPlugin.Instance.Config.Reload();
                response = "Reloaded BepInEx config successfully.";
                break;
            case "server":
                Globals.DedicatedServerManagerInstance.ReloadConfig(null, null);
                response = "Reloaded server config successfully!";
                break;
            case "both":
                Globals.DedicatedServerManagerInstance.ReloadConfig(null, null);
                GwServerPlugin.Instance.Config.Reload();
                response = "Reloaded both configs successfully!";
                break;
            default:
                response = $"Unknown config source '{args[0]}'. Validation was not called correctly.";
                return UniTask.FromResult(ChatCommandResult.ClientError(response));
        }
        return UniTask.FromResult(ChatCommandResult.Ok(response));
    }
}