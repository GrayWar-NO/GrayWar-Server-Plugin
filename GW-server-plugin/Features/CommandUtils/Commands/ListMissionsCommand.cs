using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
using GW_server_plugin.Patches;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to list missions on the server
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class ListMissionsCommand(ConfigFile config): PermissionConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string Name => "missions";

    /// <inheritdoc />
    public override string Description => "List all currently available missions";

    /// <inheritdoc />
    public override string Usage => "missions (takes no arguments)";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(args.Length == 0);
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Execute(args);

    /// <inheritdoc />
    public async UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var missions = MissionService.GetAllAvailableMissionOptions();
        if (missions.Length == 0)
        {
            return (true, "No available missions");
        }

        var response = "Available missions:\n";
        for (var i = 0; i < missions.Length; i++)
        {
            var name = missions[i].Key.Name;
            GwServerPlugin.Logger.LogDebug($"1: {name}");
            if (ulong.TryParse(name, out var id))
            {
                GwServerPlugin.Logger.LogDebug($"2: {id}");
                var missionNameResult = await MissionNameFix.GetMissionNameAsync(id);
                if (missionNameResult.success)
                {
                    GwServerPlugin.Logger.LogDebug($"3: {missionNameResult.name}");
                    name = missionNameResult.name!;
                }
                GwServerPlugin.Logger.LogDebug($"2: {name}");
            }
            response += $"[{i}] {name}\n";
        }
        return (true, response);
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Everyone;
}