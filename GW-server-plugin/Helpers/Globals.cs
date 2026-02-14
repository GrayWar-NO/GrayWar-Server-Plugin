using System;
using System.Collections.Generic;
using Mirage;
using NuclearOption.Chat;
using NuclearOption.DedicatedServer;
using NuclearOption.Networking;

namespace GW_server_plugin.Helpers;

/// <summary>
///     Has property getters for various Nuclear Option values that are used throughout the plugin.
/// </summary>
public static class Globals
{
    /// <summary>
    ///     Gets the <see cref="NetworkManagerNuclearOption" /> instance.
    /// </summary>
    public static NetworkManagerNuclearOption NetworkManagerNuclearOptionInstance => NetworkManagerNuclearOption.i ?? throw new NullReferenceException("NetworkManagerNuclearOption instance is null.");

    /// <summary>
    ///     Gets the <see cref="MissionManager" /> instance.
    /// </summary>
    public static MissionManager MissionManagerInstance => MissionManager.i ?? throw new NullReferenceException("MissionManager instance is null.");

    /// <summary>
    ///     Gets the <see cref="ChatManager" /> instance.
    /// </summary>
    public static ChatManager ChatManagerInstance => ChatManager.i ?? throw new NullReferenceException("ChatManager instance is null.");

    /// <summary>
    ///     Gets the <see cref="AudioMixerVolume" /> instance.
    /// </summary>
    public static AudioMixerVolume AudioMixerVolumeInstance => SoundManager.i.Volumes ?? throw new NullReferenceException("SoundManager AudioMixerVolume instance is null.");

    /// <summary>
    ///     Gets a read-only list of all authenticated players.
    /// </summary>
    public static IReadOnlyList<INetworkPlayer> AuthenticatedPlayers => NetworkManagerNuclearOptionInstance.Server.AuthenticatedPlayers;
    
    /// <summary>
    ///     Gets the <see cref="DedicatedServerManager" /> instance.
    /// </summary>
    public static DedicatedServerManager DedicatedServerManagerInstance => NetworkManagerNuclearOptionInstance.DedicatedServerManager ?? throw new NullReferenceException("No Dedicated Server found");   
}