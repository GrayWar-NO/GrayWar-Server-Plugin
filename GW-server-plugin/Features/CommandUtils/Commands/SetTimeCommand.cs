using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using NuclearOption.Networking;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Set time to specific hour
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class SetTimeCommand(ConfigFile config) : PermissionConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string Name { get; } = "settime";
    /// <inheritdoc />
    public override string Description { get; } = "set the time";
    /// <inheritdoc />
    public override string Usage { get; } = $"{PluginConfig.CommandPrefixChar}settime <0-24hrs> (e.g '{PluginConfig.CommandPrefixChar}settime 18' for 18:00)";
    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args)
    {
        if (args.Length != 1)
            return UniTask.FromResult(false);
        if ((args.Length == 1 && !int.TryParse(args[0], out _)) || (args.Length == 1 && int.Parse(args[0]) < 0) || (args.Length == 1 && int.Parse(args[0]) > 24))
        {
            ChatService.SendPrivateChatMessage("Number invalid. Please Try again.", player);
            return UniTask.FromResult(false);
        }

        return UniTask.FromResult(true);
    }
    
    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        if (args.Length != 1)
            return UniTask.FromResult(false);
        if ((args.Length == 1 && !int.TryParse(args[0], out _)) || (args.Length == 1 && int.Parse(args[0]) < 0) || (args.Length == 1 && int.Parse(args[0]) > 24))
            return UniTask.FromResult(false);

        return UniTask.FromResult(true);
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Execute(args);


    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var timeOfDay = int.Parse(args[0]);
        LevelInfo.i.SetTimeOfDay(timeOfDay);
        var message = $"Time set to {timeOfDay}:00";
        ChatService.SendChatMessageAsServer(message);
        return UniTask.FromResult<(bool success, string? response)>((true, null));
    }
}