using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
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
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var missions = MissionService.GetAllAvailableMissionOptions();
        if (missions.Length == 0)
        {
            return UniTask.FromResult<(bool, string?)>((true, "No available missions"));
        }

        var response = "Available missions:\n";
        for (var i = 0; i < missions.Length; i++)
        {
            response += $"[{i}] {missions[i].Key.Name}\n";
        }
        return UniTask.FromResult<(bool, string?)>((true, response));
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Everyone;
}