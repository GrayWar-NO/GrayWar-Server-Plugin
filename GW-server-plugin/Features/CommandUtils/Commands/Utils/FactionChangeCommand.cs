using System.Linq;
using BepInEx.Configuration;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Utils;

/// <summary>
/// Admin command for switching the invoking player's faction after reconnecting.
/// </summary>
[AutoCommand]
public class FactionChangeCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand
{
    /// <inheritdoc />
    public override string Name => "factionswitch";

    /// <inheritdoc />
    public override string Description => "Switch to the other faction after reconnecting.";

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
    
    /// <inheritdoc />
    public ChatCommandResult Execute(ChatCommandRequest request)
    {
        var player = request.Sender.player;
        var currentHq = player.HQ;
        if (currentHq == null)
        {
            return ChatCommandResult.ClientError("You must be in a faction before you can switch factions.");
        }

        var mission = MissionManager.CurrentMission;
        if (mission == null)
        {
            return ChatCommandResult.ClientError("Cannot switch factions because no mission is currently loaded.");
        }

        var newHq = mission.factions
            .Select(faction => faction.FactionHQ)
            .FirstOrDefault(hq => hq != null && hq != currentHq && !hq.preventJoin);

        if (newHq == null)
        {
            return ChatCommandResult.ClientError("There is no other joinable faction to switch to.");
        }

        // Match the game's reconnect flow: RemovePlayer saves the player state,
        // then Player.OnStartServer restores it with SetFaction on reconnect.
        currentHq.RemovePlayer(player);
        player.GetAuthData().SaveData.Faction = newHq;

        // NetworkManagerNuclearOption.OnServerDisconnect would otherwise remove
        // and save the old HQ a second time, overwriting the requested faction.
        player.HQ = null;

        return ChatCommandResult.Ok($"Your faction will change to {newHq.faction.factionName} when you reconnect. Please relog now.");
    }
}
