using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands.Mission;

/// <summary>
///     Command to list missions on the server
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class ListMissionsCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
    
    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args) => UniTask.FromResult(args.Length == 0);
    
    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var missions = MissionService.GetAllAvailableMissionOptions();
        if (missions.Length == 0) return UniTask.FromResult((true, "No available missions"))!;
        
        var response = "Available missions:\n";
        for (var i = 0; i < missions.Length; i++)
        {
            var name = missions[i].Key.TryGetKey(out var key) ? key.Name : missions[i].Key.Name;
            response += $"[{i}] {name}\n";
        }
        
        return UniTask.FromResult((true, response))!;
    }
    
    /// <inheritdoc />
    public override string OutputName => "missions";
    
    /// <inheritdoc />
    public override string Description => "List all currently available missions";
    
    /// <inheritdoc />
    public override string Usage => "missions (takes no arguments)";
    
    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);
    
    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Execute(args);
}