using BepInEx.Configuration;
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
        new("amount", ChatArgumentType.Text),// TODO waiting for FLOAT type
    ];
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
    
    /// <inheritdoc />
    public override ChatCommandResult Execute(string[] args)
    {
        var weatherFloat = float.Parse(args[0]);

        LevelInfo.i.Networkconditions = weatherFloat;
        ChatService.SendChatMessageAsServer($"Weather set to {weatherFloat}");
        return ChatCommandResult.Ok();
    }
}