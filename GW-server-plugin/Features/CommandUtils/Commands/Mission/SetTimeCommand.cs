using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands.Mission;

/// <summary>
/// Set time to specific hour
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class SetTimeCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string OutputName => "settime";
    
    /// <inheritdoc />
    public override string Description => "set the time of day";
    
    /// <inheritdoc />
    public override string Usage => $"settime <0-24hrs> (e.g '{PluginConfig.CommandPrefixChar}settime 18' for 18:00)";
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);
    
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
        return UniTask.FromResult<(bool success, string? response)>((true, message));
    }
}