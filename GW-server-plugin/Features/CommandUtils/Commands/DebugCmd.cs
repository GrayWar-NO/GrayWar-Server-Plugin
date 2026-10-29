#if DEBUG

using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to use exclusively for debugging. Change this implementation to debug whatever you're working on.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class DebugCmd(ConfigFile config): CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string Name => "dbg";

    /// <inheritdoc />
    public override string Description => "debug-only command";

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
    
    /// <inheritdoc />
    public override UniTask<ChatCommandResult> Execute(string[] args)
    {
        var dsm = Globals.DedicatedServerManagerInstance;
        return UniTask.FromResult(ChatCommandResult.Ok($"DSMtf:{dsm.currentMission.environment.timeFactor}"));
    }
}
#endif
