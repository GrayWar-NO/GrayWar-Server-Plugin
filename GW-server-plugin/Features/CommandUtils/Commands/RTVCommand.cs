using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to vote on changing the mission.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class RtvCommand(ConfigFile config): PermissionConfigurableCommand(config), IGameCommand
{
    private static readonly HashSet<string> NoValues =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "n",
            "no",
        };
    
    /// <inheritdoc />
    public override string Name => "rtv";

    /// <inheritdoc />
    public override string Description =>
        "Command to vote on queuing the next mission.\nMissionIDs for voting for a specific mission can be found with /missions.";

    /// <inheritdoc />
    public override string Usage =>
        $"rtv <missionID> or {PluginConfig.CommandPrefixChar}rtv <(N)o> to keep next mission as is";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args)
    {
        return UniTask.FromResult(args.Length == 1 && (NoValues.Contains(args[0]) || int.TryParse(args[0], out _)));
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        var missionCount = MissionService.GetAllAvailableMissionOptions().Length;
        int missionID = -1; // Mission ID will be ignored anyways if a NO vote
        var yes = true;

        if (NoValues.Contains(args[0]))
            yes = false;
        else
            missionID = int.Parse(args[0]);

        if (missionID < 0 || missionID >= missionCount)
            return UniTask.FromResult<(bool, string?)>((false, "Invalid mission ID. Pick a valid one."));

        var result = GwServerPlugin.MissionVote.RegisterRtv(player.SteamID, yes, missionID, out var registerResponse);
        var missionText = missionID == null ? "next mission in rotation" : $"mission with ID {missionID}";
        registerResponse ??= result
            ? $"You have successfully voted for the {missionText}."
            : "Your mission vote was unsuccessful.";
        return UniTask.FromResult<(bool, string?)>((result, registerResponse));
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}