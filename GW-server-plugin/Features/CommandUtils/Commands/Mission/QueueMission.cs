using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands.Mission;

/// <summary>
///     Command for queuing a mission to be loaded next on the server.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class QueueMission(ConfigFile config) : ConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string OutputName => "queue";
    
    /// <inheritdoc />
    public override string Description => "Queue a mission to be loaded next on the server.";
    
    /// <inheritdoc />
    public override string Usage => $"queue <mission ID (from {PluginConfig.CommandPrefixChar}mission)>";
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
    
    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);
    
    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Execute(args);
    
    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(args.Length switch
        {
            > 1 => false,
            <= 0 => false,
            1 => int.TryParse(args[0], out _)
        });
    }
    
    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var missionIndex = int.Parse(args[0]);
        var missionOption = MissionService.GetMissionOptionByIndex(missionIndex);
        if (missionOption == null) return UniTask.FromResult((false, "Mission not found."))!;
        {
            Globals.DedicatedServerManagerInstance.missionRotation.OverrideNext(missionOption.Value);
            return UniTask.FromResult((true, $"Queued mission {missionOption.Value.Key.Name} successfully."));
        }

    }
}