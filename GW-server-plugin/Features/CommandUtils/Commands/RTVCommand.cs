using BepInEx.Configuration;
using GW_server_plugin.Enums;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to vote on changing the mission.
/// </summary>
/// <param name="config"></param>
public class RtvCommand(ConfigFile config): PermissionConfigurableCommand(config)
{
    /// <inheritdoc />
    public override string Name { get; } = "rtv";

    /// <inheritdoc />
    public override string Description { get; } = "Command to cote on changing the mission.\nNeeds Absolute Majority.\nMissionIDs for voting for a specific mission can be found with /missions.\nChooses the next mission in rotation by default.";

    /// <inheritdoc />
    public override string Usage { get; } = "rtv <Optional int missionID>";

    /// <inheritdoc />
    public override bool Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public override bool Validate(string[] args)
    {
        if (args.Length == 0) return true;
        if (args.Length < 1) return false;
        return int.TryParse(args[0], out _);
    }

    /// <inheritdoc />
    public override bool Execute(Player player, string[] args, out string? response)
    {
        int? missionID;
        if (args.Length == 0) missionID = null;    
        else missionID = int.Parse(args[0]);
        
        var result = GwServerPlugin.MissionVote.RegisterRtv(player.SteamID, missionID);
        var missionText = missionID == null ? "next mission in rotation" : $"mission with ID {missionID}";
        response = result ? $"You have successfully voted for the {missionText}." : "Your mission vote was unsuccessful.";
        return result;
    }

    /// <inheritdoc />
    public override bool Execute(string[] args, out string? response)
    {
        response = "RTV is not available from the console. Please use nextmission instead.";
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Everyone;
}