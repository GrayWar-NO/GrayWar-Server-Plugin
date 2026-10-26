using System.Linq;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Mission;

/// <summary>
/// Set time to specific hour
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class SetTimeCommand(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string Name => "settime";
    
    /// <inheritdoc />
    public override string Description => "set the time of day";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        ChatCommandArgument.FromChoices("time", [.. Enumerable.Range(0, 24).Select(n => $"{n}:00")])
    ];
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
    
    /// <inheritdoc />
    public override UniTask<ChatCommandResult> Execute(string[] args)
    {
        var timeOfDay = int.Parse(args[0]);
        LevelInfo.i.SetTimeOfDay(timeOfDay);
        var message = $"Time set to {timeOfDay}:00";
        return UniTask.FromResult(ChatCommandResult.Ok(message));
    }
}