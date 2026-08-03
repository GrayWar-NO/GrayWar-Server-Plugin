using System;
using NuclearOption.DedicatedServer;

namespace GW_server_plugin.Features.Voting.Sessions.Missions;

/// <summary>
///     IEquatable wrapper struct for <see cref="MissionOptions"/>
/// </summary>
/// <param name="options"></param>
public readonly struct EquatableMissionOptions(MissionOptions options) : IEquatable<EquatableMissionOptions>
{
    /// <summary>
    ///     Wrapped MissionOptions
    /// </summary>
    public MissionOptions Options { get; } = options;
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is EquatableMissionOptions other && Equals(other);
    }
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Options.Key.GetHashCode();
    }
    
    /// <inheritdoc />
    public bool Equals(EquatableMissionOptions other) =>
        Options.Key.Equals(other.Options.Key) && Options.MaxTime.Equals(other.Options.MaxTime);
}