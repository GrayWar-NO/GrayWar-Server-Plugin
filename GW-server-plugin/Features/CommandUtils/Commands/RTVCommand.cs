using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to vote on changing the mission.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class RtvCommand(ConfigFile config): PermissionConfigurableCommand(config), IGameCommand
{
    private static readonly HashSet<string> YesValues =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "y",
            "yes",
        };

    private static readonly HashSet<string> NoValues =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "n",
            "no",
        };
    
    /// <inheritdoc />
    public override string Name => "rtv";

    /// <inheritdoc />
    public override string Description => "Command to vote on changing the mission.\nMissionIDs for voting for a specific mission can be found with /missions.\nChooses the next mission in rotation by default.";

    /// <inheritdoc />
    public override string Usage => "rtv <(Y)es/(N)o> <Optional int missionID>";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args)
    {
        if (args.Length is 0 or > 2 || 
            !YesValues.Contains(args[0]) && !NoValues.Contains(args[0])) return UniTask.FromResult(false);
        return UniTask.FromResult(args.Length <= 1 || int.TryParse(args[1], out _));
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        int? missionID;
        if (args.Length == 1) missionID = null;    
        else missionID = int.Parse(args[1]);
        var yes = YesValues.Contains(args[0]);
        
        var result = GwServerPlugin.MissionVote.RegisterRtv(player.SteamID, yes, missionID, out var registerResponse );
        var missionText = missionID == null ? "next mission in rotation" : $"mission with ID {missionID}";
        registerResponse ??= result ? $"You have successfully voted for the {missionText}." : "Your mission vote was unsuccessful.";
        return UniTask.FromResult<(bool, string?)>((result, registerResponse));
    }
    
    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}