using System;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.Chat;
using NuclearOption.ModdingApi;
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
        new("durationDays", ChatArgumentType.Int, true),
        new("reason", ChatArgumentType.Text, true)
    ];
    
    /// <inheritdoc />
    public async UniTask<ChatCommandResult> Execute(ChatCommandRequest request)
    {
        if (!request.TryGetTargetPlayer(0, out var target, out var error))
            return ChatCommandResult.ClientError(error);
        if (target != request.Sender) return await Execute([.. request.Args]);
        return ChatCommandResult.ClientError("You cannot ban yourself!");
    }

    /// <inheritdoc />
    public UniTask<ChatCommandResult> Execute(string[] args)
    {
        TimeSpan? duration = args.Length >= 2 ? TimeSpan.FromDays(int.Parse(args[1])) : null;
        var reason = args.Length == 3 ? args[2] : "Unknown reason";
        
        if (!ChatHelper.TryParseSteamId(args[0], out var banSteamID))
            return UniTask.FromResult(ChatCommandResult.ClientError($"{args[0]} is not a valid player."));
        if (UnitRegistry.playerLookupSteamId.TryGetValue(banSteamID, out var playerObject))
            PlayerUtils.KickPlayer(playerObject, "Banned for " + reason);
        var response = $"Banned player {playerObject?.MentionArg ?? banSteamID.m_SteamID.ToString()} for {reason}";
        
        if (duration is not null)
        {
            response += $" for {duration}.";
        }

        PlayerUtils.BanPlayer(banSteamID.m_SteamID, reason, duration);
        return UniTask.FromResult(ChatCommandResult.Ok(response));
    }

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Moderator;
}
