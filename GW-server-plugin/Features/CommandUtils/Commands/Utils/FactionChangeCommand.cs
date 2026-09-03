using System.Linq;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands.Utils;

/// <summary>
/// Admin command for switching the invoking player's faction after reconnecting.
/// </summary>
[AutoCommand]
public class FactionChangeCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string OutputName => "factionswitch";

    /// <inheritdoc />
    public override string Description => "Switch to the other faction after reconnecting.";

    /// <inheritdoc />
    public override string Usage => "factionswitch";

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => UniTask.FromResult(args.Length == 0);

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args) => UniTask.FromResult(args.Length == 0);

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        var currentHq = player.HQ;
        if (currentHq == null)
        {
            return UniTask.FromResult<(bool, string?)>((false,
                "You must be in a faction before you can switch factions."));
        }

        var mission = MissionManager.CurrentMission;
        if (mission == null)
        {
            return UniTask.FromResult<(bool, string?)>((false,
                "Cannot switch factions because no mission is currently loaded."));
        }

        var newHq = mission.factions
            .Select(faction => faction.FactionHQ)
            .FirstOrDefault(hq => hq != null && hq != currentHq && !hq.preventJoin);

        if (newHq == null)
        {
            return UniTask.FromResult<(bool, string?)>((false,
                "There is no other joinable faction to switch to."));
        }

        // Match the game's reconnect flow: RemovePlayer saves the player state,
        // then Player.OnStartServer restores it with SetFaction on reconnect.
        currentHq.RemovePlayer(player);
        player.GetAuthData().SaveData.Faction = newHq;

        // NetworkManagerNuclearOption.OnServerDisconnect would otherwise remove
        // and save the old HQ a second time, overwriting the requested faction.
        player.HQ = null;

        return UniTask.FromResult<(bool, string?)>((true,
            $"Your faction will change to {newHq.faction.factionName} when you reconnect. Please relog now."));
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        return UniTask.FromResult<(bool, string?)>((false,
            "This command must be run by an in-game player."));
    }
}
