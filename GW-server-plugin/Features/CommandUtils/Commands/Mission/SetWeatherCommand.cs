using System.Linq;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Mission;

/// <summary>
/// Set weather to specific value
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class SetWeatherCommand(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string Name => "setweather";
    
    /// <inheritdoc />
    public override string Description => "set the weather";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        ChatCommandArgument.FromChoices("weather", [.. LevelInfo.i.cloudLayer.weatherSets.Select(ws => ws.displayName)])
    ];
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
    
    /// <inheritdoc />
    public override UniTask<ChatCommandResult> Execute(string[] args)
    {
        
        var weatherSet = LevelInfo.i.cloudLayer.weatherSets.FirstOrDefault(ws => ws.displayName == args[0]);
        var weatherFloat = weatherSet.coverage;
        
        LevelInfo.i.Networkconditions = weatherFloat;
        return UniTask.FromResult(ChatCommandResult.Ok($"Weather set to {weatherSet.displayName}"));
    }
}