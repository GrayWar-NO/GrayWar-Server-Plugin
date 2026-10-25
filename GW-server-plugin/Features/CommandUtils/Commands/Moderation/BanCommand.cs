using System.Linq;
using System.Security;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.ModdingApi;
using NuclearOption.Networking;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Moderation;

/// <summary>
/// Command to ban a player.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class BanCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string Name => "ban";
    
    /// <inheritdoc />
    public override string Description => "Bans a player from the server.";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("player", ChatArgumentType.Player),
        new("reason", ChatArgumentType.Text, true),
        new("duration (in [d]ays or [h]ours)", ChatArgumentType.Text, true)
    ];
    
    /// <inheritdoc />
    public ChatCommandResult Execute(ChatCommandRequest request)
    {
        var args = request.Args;
        var player = request.Sender.player;
        
        var target = args[0];
        PlayerUtils.TryFindPlayer(target, out var targetPlayer);
        if (targetPlayer != player) return Execute([.. args]);
        return ChatCommandResult.ClientError("You cannot ban yourself!");
    }

    /// <inheritdoc />
    public ChatCommandResult Execute(string[] args)
    {
        var target = args[0];
        string? duration = null;
        string reason;
        if (args.Length > 1)
        {
            var tmp = args[args.Length - 1].Last();
            duration = tmp is 'd' or 'h' ? args[args.Length - 1] : null;
            var reasonEnum = args.Skip(1).ToList();
            if (duration is not null)
            {
                reasonEnum.RemoveAt(reasonEnum.Count - 1);
            }

            reason = string.Join(" ", reasonEnum);
        }
        else
        {
            reason = "Unknown reason";
        }

        string? response;

        ulong banSteamID;
        if (ulong.TryParse(target, out var targetID) &&
            targetID > (ulong)Globals.DedicatedServerManagerInstance.Config.MaxPlayers)
        {
            banSteamID = targetID;
            response = $"Banned player with steamID {banSteamID} for reason {reason}";
            if (PlayerUtils.TryFindPlayerBySteamId(banSteamID, out var targetPlayer))
            {
                Globals.NetworkManagerNuclearOptionInstance
                    .KickPlayerAsync(targetPlayer!, $"Banned for reason: {reason}").Forget();
            }
        }
        else
        {
            var rs = PlayerUtils.TryFindPlayer(target, out var player);
            if (!rs)
                throw new VerificationException(
                    $"Could not find player {target}: validation was not called properly.");
            banSteamID = player!.SteamID;
            Globals.NetworkManagerNuclearOptionInstance
                .KickPlayerAsync(player, $"Banned for reason: {reason}").Forget();
            response = $"Banned player {player.GetDisplayName()} for reason {reason}";
        }

        if (duration is not null)
        {
            response += $" for {duration}.";
        }

        PlayerUtils.BanPlayer(banSteamID, reason, duration);
        return ChatCommandResult.Ok(response);
    }

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
}
