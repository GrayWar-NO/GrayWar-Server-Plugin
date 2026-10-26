using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.Chat;
using NuclearOption.ModdingApi;
using Steamworks;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Moderation;

/// <summary>
/// Command to kick a player from the server
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class UnKickCommand(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string Name => "unkick";

    /// <inheritdoc />
    public override string Description => "Unkicks a player from the  server.";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("player (steamID)", ChatArgumentType.Player)
    ];
    
    /// <inheritdoc />
    public override UniTask<ChatCommandResult> Execute(string[] args)
    {
        if (!ChatHelper.TryParseSteamId(args[0], out var targetSteamID))
            return UniTask.FromResult(ChatCommandResult.ClientError($"Invalid User: {args[0]}"));

        UnKickPlayer(targetSteamID);
        return UniTask.FromResult(ChatCommandResult.Ok($"Unkicked player with steamID {args[0]}"));
    }

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;

    private static void UnKickPlayer(CSteamID steamID)
    {
        Globals.NetworkManagerNuclearOptionInstance.Authenticator.KickList.Remove(steamID);
        Globals.NetworkManagerNuclearOptionInstance.Authenticator.MissionKickList.Remove(steamID);
    }
}