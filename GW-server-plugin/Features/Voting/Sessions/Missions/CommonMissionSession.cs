using System; 
using System.Linq;
using BepInEx.Configuration;
using GW_server_plugin.Helpers;
using GW_server_plugin.Patches;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.Voting.Sessions.Missions;

/// <summary>
///     Session type for voteSessions that deal with missions.
/// </summary>
/// <typeparam name="TSession"></typeparam>
public abstract class CommonMissionSession<TSession>
    : ConfigurableVoteSession<TSession, EquatableMissionOptions>
    where TSession : ConfigurableVoteSession<TSession, EquatableMissionOptions>
{
    /// <inheritdoc />
    protected CommonMissionSession(Player initiator, string? reason) :
        base(initiator, reason)
    {
        var acceptableValuesArray = Globals.DedicatedServerManagerInstance.missionRotation.allMissions
            .Select(av => new EquatableMissionOptions(av)).ToArray();
        AcceptableValues = new AcceptableValueList<EquatableMissionOptions>(acceptableValuesArray);
    }
    
    /// <inheritdoc />
    protected sealed override AcceptableValueBase AcceptableValues { get; }
    
    
    /// <inheritdoc />
    protected override bool TryParseValue(string input, out EquatableMissionOptions? result)
    {
        result = null;
        if (AcceptableValues is AcceptableValueList<EquatableMissionOptions> avl)
        {
            if (uint.TryParse(input, out var index) && index < avl.AcceptableValues.Length)
            {
                result = avl.AcceptableValues[index];
                return true;
            }
            
            var validValues = avl.AcceptableValues.Where(m => ValueStringGetter(m) == input).ToList();
            if (!validValues.Any()) return false;
            result = validValues.First();
            return true;
        }
        
        GwServerPlugin.Logger.LogError("AcceptableValues is not the correct type in SkipSession. What the fuck??");
        return false;
    }
    
    /// <inheritdoc />
    protected override string ValueStringGetter(EquatableMissionOptions value)
    {
        var avl = (AcceptableValues as AcceptableValueList<EquatableMissionOptions>)!.AcceptableValues;
        var index = avl.IndexOf(value);
        if (!value.Options.Key.TryGetKey(out var key)) return value.Options.Key.Name;
        key = MissionNameFix.TranslateWorkshopName(key);
        return $"[{index}] {key.Name}";
    }
}