using System.Linq;
using BepInEx.Configuration;
using GW_server_plugin.Helpers;
using GW_server_plugin.Patches;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.Voting.Sessions.Missions;

/// <summary>
///     Session for voteSkip.
/// </summary>
[AutoVoteSession("Mission Skip", "skip")]
public sealed class SkipSession
    : ConfigurableVoteSession<SkipSession, EquatableMissionOptions>
{
    /// <inheritdoc />
    public SkipSession(Player initiator, string? reason) :
        base(initiator, reason)
    {
        var acceptableValuesArray = Globals.DedicatedServerManagerInstance.missionRotation.allMissions
            .Select(av => new EquatableMissionOptions(av)).ToArray();
        AcceptableValues = new AcceptableValueList<EquatableMissionOptions>(acceptableValuesArray);
    }
    
    /// <inheritdoc />
    protected override AcceptableValueBase AcceptableValues { get; }
    
    /// <inheritdoc />
    protected override EquatableMissionOptions? DefaultVote { get; } =
        new(MissionService.GetNextMissionOptions(false)!.Value);
    
    /// <inheritdoc />
    protected override string ValueStringGetter(EquatableMissionOptions value)
    {
        if (!value.Options.Key.TryGetKey(out var key)) return value.Options.Key.Name;
        key = MissionNameFix.TranslateWorkshopName(key);
        return key.Name;
    }
    
    /// <inheritdoc />
    protected override void OnPass(EquatableMissionOptions outcome)
    {
        _ = MissionService.StartMission(outcome.Options);
    }
    
    /// <inheritdoc />
    protected override void OnFail()
    {
    }
    
    /// <inheritdoc />
    protected override bool TryParseValue(string input, out EquatableMissionOptions? result)
    {
        result = null;
        if (AcceptableValues is AcceptableValueList<EquatableMissionOptions> avl)
        {
            var validValues = avl.AcceptableValues.Where(m => ValueStringGetter(m) == input).ToList();
            if (!validValues.Any()) return false;
            result = validValues.First();
            return true;
        }
        GwServerPlugin.Logger.LogError("AcceptableValues is not the correct type in SkipSession. What the fuck??");
        return false;
    }
}