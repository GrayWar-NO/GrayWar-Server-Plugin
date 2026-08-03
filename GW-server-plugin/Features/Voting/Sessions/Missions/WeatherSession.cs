using System;
using System.Linq;
using BepInEx.Configuration;

namespace GW_server_plugin.Features.Voting.Sessions.Missions;

/// <summary>
///     VoteSession for weather
/// </summary>
[AutoVoteSession("Weather Intenity", "weather")]
public class WeatherSession : ConfigurableVoteSession<WeatherSession, EquatableWeatherSet>
{
    /// <inheritdoc />
    public WeatherSession(string? response) : base(response)
    {
        var acceptableValuesArray =
            LevelInfo.i.cloudLayer.weatherSets.Select(s => new EquatableWeatherSet(s)).ToArray();
        AcceptableValues = new AcceptableValueList<EquatableWeatherSet>(acceptableValuesArray);
    }
    
    /// <inheritdoc />
    protected override AcceptableValueBase AcceptableValues { get; }
    
    /// <inheritdoc />
    protected override EquatableWeatherSet? DefaultVote => null;
    
    /// <inheritdoc />
    protected override string ValueStringGetter(EquatableWeatherSet value) => value.Set.displayName.ToLower();
    
    /// <inheritdoc />
    protected override void OnPass(EquatableWeatherSet outcome)
    {
        var cloudLayer = LevelInfo.i.cloudLayer;
        if (cloudLayer?.weatherSets == null || cloudLayer.weatherSets.Length == 0)
            return;
        
        // Find the index of the chosen weather set
        var index = Array.IndexOf(cloudLayer.weatherSets, outcome.Set);
        if (index == -1)
            index = 0;
        
        var length = cloudLayer.weatherSets.Length;
        
        // Calculate a conditions value that maps to this specific index.
        // Using (index + 0.5f) targets the middle of the valid range for this index.
        var targetConditions = (index + 0.5f) / length;
        
        // Apply it to the network conditions property
        LevelInfo.i.Networkconditions = targetConditions;
    }
    
    /// <inheritdoc />
    protected override void OnFail()
    {
    }
    
    /// <inheritdoc />
    protected override bool TryParseValue(string input, out EquatableWeatherSet? result)
    {
        result = null;
        if (AcceptableValues is not AcceptableValueList<EquatableWeatherSet> avl) return false;
        var validValues = avl.AcceptableValues.Where(v => input == ValueStringGetter(v)).ToList();
        if (!validValues.Any()) return false;
        result = validValues.First();
        return true;
    }
}

/// <summary>
///     IEquatable wrapper for WeatherSet
/// </summary>
public readonly struct EquatableWeatherSet(WeatherSet weatherSet) : IEquatable<EquatableWeatherSet>
{
    /// <summary>
    ///     Wrapped WeatherSet
    /// </summary>
    public WeatherSet Set { get; } = weatherSet;
    
    /// <inheritdoc />
    public bool Equals(EquatableWeatherSet other)
    {
        return Set.Equals(other.Set);
    }
}