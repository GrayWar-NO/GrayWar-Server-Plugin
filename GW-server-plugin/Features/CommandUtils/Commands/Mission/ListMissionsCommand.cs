using BepInEx.Configuration;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Mission;

/// <summary>
///     Command to list missions on the server
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class ListMissionsCommand(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
    
    /// <inheritdoc />
    public override ChatCommandResult Execute(string[] args)
    {
        var missions = MissionService.GetAllAvailableMissionOptions();
        if (missions.Length == 0) return ChatCommandResult.Ok("No available missions")!;
        
        var response = "Available missions:\n";
        for (var i = 0; i < missions.Length; i++)
        {
            var name = missions[i].Key.TryGetKey(out var key) ? key.Name : missions[i].Key.Name;
            response += $"[{i}] {name}\n";
        }
        
        return ChatCommandResult.Ok(response)!;
    }
    
    /// <inheritdoc />
    public override string Name => "missions";
    
    /// <inheritdoc />
    public override string Description => "List all currently available missions";
}