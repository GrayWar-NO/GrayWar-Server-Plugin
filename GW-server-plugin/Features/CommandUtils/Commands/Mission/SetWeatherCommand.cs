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
        new("amount", ChatArgumentType.Float)
    ];
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
    
    /// <inheritdoc />
    public override UniTask<ChatCommandResult> Execute(string[] args)
    {
        var weatherFloat = float.Parse(args[0]);
        if (weatherFloat is < 0f or > 1f)
            return UniTask.FromResult(ChatCommandResult.ClientError($"{weatherFloat} is not between 0 and 1"));
        
        LevelInfo.i.Networkconditions = weatherFloat;
        return UniTask.FromResult(ChatCommandResult.Ok($"Weather set to {weatherFloat}."));
    }
}