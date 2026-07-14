using System.Linq;
using System.Security;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to ban a player.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class WarnCommand(ConfigFile config): PermissionConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string Name { get; } = "warn";

    /// <inheritdoc />
    public override string Description { get; } = "Warns a player.";

    /// <inheritdoc />
    public override string Usage { get; } = "warn <Player (by name, steamID or playerID)> <Reason>";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(args.Length >= 1 && (PlayerUtils.TryFindPlayer(args[0], out _) || ulong.TryParse(args[0], out _)));
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        var target = args[0];
        PlayerUtils.TryFindPlayer(target, out var targetPlayer);
        if (targetPlayer != player) return Execute(args);
        return UniTask.FromResult<(bool, string?)>((false, "You cannot warn yourself!"));
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var target = args[0];
        var reason = string.Join(" ", args.Skip(1));
        string? response;
        ulong warnSteamID;
        if (ulong.TryParse(target, out var targetID) && targetID > (ulong)Globals.DedicatedServerManagerInstance.Config.MaxPlayers)
        {
            warnSteamID = targetID;
            response = $"Warned player with steamID {warnSteamID} for reason {reason}";
        }
        else
        {
            var rs = PlayerUtils.TryFindPlayer(target, out var player);
            if (!rs)
                throw new VerificationException(
                    $"Could not find player {target}: validation was not called properly.");
            warnSteamID = player!.SteamID;
            response = $"Warned player {player.PlayerName} for reason {reason}";
        }

        return UniTask.FromResult<(bool, string?)>((GwServerPlugin.WarnService.AddWarn(warnSteamID, reason), response));
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
}