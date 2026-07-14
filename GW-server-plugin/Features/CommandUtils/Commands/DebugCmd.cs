#if DEBUG

using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using Com.Graywar.NoServerManager.Proto;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to use exclusively for debugging. Change this implementation to debug whatever you're working on.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class DebugCmd(ConfigFile config): PermissionConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string Name => "dbg";

    /// <inheritdoc />
    public override string Description => "debug-only command";

    /// <inheritdoc />
    public override string Usage => "take a guess bro";

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args)
    {
        return UniTask.FromResult(true);
    }
    
    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(true);
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Execute(args);

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var dsm = Globals.DedicatedServerManagerInstance;
        return UniTask.FromResult((true, $"DSMtf:{dsm.currentMission.environment.timeFactor}"));
    }
}
#endif
