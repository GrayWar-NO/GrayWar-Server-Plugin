using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;

namespace GW_server_plugin.Features.CommandUtils.Commands.Mission;

/// <summary>
/// Set weather to specific value
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class SetWeatherCommand(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string OutputName => "setweather";
    
    /// <inheritdoc />
    public override string Description => "set the weather";
    
    /// <inheritdoc />
    public override string Usage => $"setweather <0-1>";
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
    
    /// <inheritdoc />
    public override UniTask<bool> Validate(string[] args)
    {
        if (args.Length != 1)
            return UniTask.FromResult(false);
        if (!float.TryParse(args[0], out _) || float.Parse(args[0]) < 0 || float.Parse(args[0]) > 1)
            return UniTask.FromResult(false);

        return UniTask.FromResult(true);
    }
    
    /// <inheritdoc />
    public override UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var weatherFloat = float.Parse(args[0]);

        LevelInfo.i.Networkconditions = weatherFloat;
        ChatService.SendChatMessageAsServer($"Weather set to {weatherFloat}");
        return UniTask.FromResult<(bool success, string? response)>((true, null));
    }
}