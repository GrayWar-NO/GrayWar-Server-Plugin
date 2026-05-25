#if DEBUG

using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

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
    public bool Validate(Player player, string[] args)
    {
        return true;
    }
    
    /// <inheritdoc />
    public bool Validate(string[] args)
    {
        return true;
    }

    /// <inheritdoc />
    public bool Execute(Player player, string[] args, out string? response) => Execute(args, out response);

    /// <inheritdoc />
    public bool Execute(string[] args, out string? response)
    {
        var mm = Globals.MissionManagerInstance;
        var dsm = Globals.DedicatedServerManagerInstance;
        response = $"DSMtf:{dsm.currentMission.environment.timeFactor}";
        return true;
    }
}
#endif
