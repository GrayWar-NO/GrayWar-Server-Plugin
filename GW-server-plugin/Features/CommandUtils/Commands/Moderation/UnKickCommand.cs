using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
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
        new("player (steamID)", ChatArgumentType.Text) // TODO wait for ulong type
    ];
    
    /// <inheritdoc />
    public override UniTask<ChatCommandResult> Execute(string[] args)
    {
        UnKickPlayer(ulong.Parse(args[0]));
        return UniTask.FromResult(ChatCommandResult.Ok($"Unkicked player with steamID {args[0]}"));
    }

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;

    private static void UnKickPlayer(ulong steamID)
    {
        Globals.NetworkManagerNuclearOptionInstance.Authenticator.KickList.Remove(new CSteamID(steamID));
        Globals.NetworkManagerNuclearOptionInstance.Authenticator.MissionKickList.Remove(new CSteamID(steamID));
    }
}