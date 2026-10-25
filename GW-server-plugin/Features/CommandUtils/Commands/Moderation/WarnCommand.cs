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
public class WarnCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string Name => "warn";
    
    /// <inheritdoc />
    public override string Description => "Warns a player.";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("player", ChatArgumentType.Player),
        new("reason", ChatArgumentType.Text)
    ];
    
    /// <inheritdoc />
    public ChatCommandResult Execute(ChatCommandRequest request)
    {
        var args = request.Args;
        var player = request.Sender.player;
        
        var target = args[0];
        PlayerUtils.TryFindPlayer(target, out var targetPlayer);
        if (targetPlayer != player) return Execute([.. args]);
        return ChatCommandResult.ClientError("You cannot warn yourself!");
    }
    
    /// <inheritdoc />
    public ChatCommandResult Execute(string[] args)
    {
        var target = args[0];
        var reason = string.Join(" ", args.Skip(1));
        string? response;
        ulong warnSteamID;
        if (ulong.TryParse(target, out var targetID) &&
            targetID > (ulong)Globals.DedicatedServerManagerInstance.Config.MaxPlayers)
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
            response = $"Warned player {player.GetDisplayName()} for reason {reason}";
        }
        
        return GwServerPlugin.WarnService.AddWarn(warnSteamID, reason)
            ? ChatCommandResult.Ok(response)
            : ChatCommandResult.ClientError($"failed to warn {target}: is no longer on server.");
    }
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}