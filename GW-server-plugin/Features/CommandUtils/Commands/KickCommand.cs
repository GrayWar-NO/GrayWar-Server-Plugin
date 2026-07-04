using System.Linq;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to kick a player from the server
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class KickCommand(ConfigFile config): PermissionConfigurableCommand(config), IConsoleCommand, IGameCommand
{
    /// <inheritdoc />
    public override string Name { get; } = "kick";

    /// <inheritdoc />
    public override string Description { get; } = "Kicks a player from the  server.";

    /// <inheritdoc />
    public override string Usage { get; } = "kick <player (by name, steamID or ID tag)> <reason>";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(
            args.Length >= 1 &&
            (PlayerUtils.TryFindPlayer(args[0], out _) || ulong.TryParse(args[0], out _))
            );
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        var target = args[0];
        PlayerUtils.TryFindPlayer(target, out var targetPlayer);
        if (targetPlayer != player) return Execute(args);
        return UniTask.FromResult<(bool, string?)>((false, "You cannot kick yourself!"));
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var target = args[0];
        if (PlayerUtils.TryFindPlayer(target, out var targetPlayer))
        {
            PlayerUtils.KickPlayer(targetPlayer!,  string.Join(" ", args.Skip(1)));
            return UniTask.FromResult((true, $"{targetPlayer!.PlayerName} has been kicked!"));
        }

        return UniTask.FromResult((false, $"{target} is not a valid player!"));
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
    
}