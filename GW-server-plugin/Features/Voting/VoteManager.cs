using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using JetBrains.Annotations;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.Voting;

/// <summary>
///     Manages vote sessions, starting vote sessions and the like.
/// </summary>
public static class VoteManager
{
    /// <summary>
    ///     Currently in use votesession or null
    /// </summary>
    public static IVoteSession? Session;
    
    /// <summary>
    ///     Initialises all the voteSession configs.
    /// </summary>
    /// <param name="pluginConfig"></param>
    public static void Initialize(ConfigFile pluginConfig)
    {
        // Load all the existing sessions via Reflection
        
        var baseOpenType = typeof(ConfigurableVoteSession<,>);
        var assembly = Assembly.GetAssembly(baseOpenType)!;
        
        var sessionTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Select(t => (Type: t, Attr: t.GetCustomAttribute<AutoVoteSessionAttribute>()))
            .Where(x => x.Attr != null);
        
        var sessionTypesList = sessionTypes.ToList();
        GwServerPlugin.Logger.LogDebug($"Found {sessionTypesList.Count} session types");
        
        foreach (var (sessionType, sessionAttr) in sessionTypesList)
        {
            GwServerPlugin.Logger.LogDebug($"Initializing vote session for {sessionType.Name}");

            var categoryName = $"{sessionAttr.SessionName} vote session";
            var initMethod = sessionType.BaseType?.GetMethod("InitializeConfig",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (initMethod == null)
            {
                GwServerPlugin.Logger.LogWarning($"Failed to initialize vote session for {sessionType.Name}: InitializeConfig method not found");
                continue;
            }
            
            initMethod.Invoke(null, [pluginConfig, categoryName]);
            Factories[sessionAttr.ShortName] = s => (IVoteSession)Activator.CreateInstance(sessionType, s);
            GwServerPlugin.Logger.LogDebug($"Initialized vote session for {sessionType.Name} successfully");
        }
    }    
    /// <summary>
    ///     Starts a voteSession.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="initiator"></param>
    /// <param name="outcome"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    public static bool TryStartVote(IVoteSession session, Player initiator, string outcome, out string? response)
    {
        response = null;
        if (Session != null)
        {
            response = $"Cannot start vote: there is already a {Session.SessionName} vote ongoing.";
            return false;
        }
        Session = session;
        Session.Start(initiator);
        return Session.TryAddVote(initiator, outcome, out response);
    }
    
    /// <summary>
    ///     VoteSession factories that exist.
    /// </summary>
    public static readonly Dictionary<string, Func<string?, IVoteSession>> Factories = new();    
    
    /// <summary>
    ///     Destroys the current voteSession.
    /// </summary>
    public static void CancelVote()
    {
        Session?.Destroy();
        Session = null;
    }
}

/// <summary>
/// Attribute to mark a voteSession as implicitly used by the Reflection discovery in the VoteManager class.
/// </summary>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class)]
public sealed class AutoVoteSessionAttribute(string sessionName, string shortName) : Attribute
{
    /// <summary>
    ///     ShortName for the VoteSession
    /// </summary>
    public string ShortName { get; } =shortName;
    
    /// <summary>
    ///     SessionName for the voteSession
    /// </summary>
    public string SessionName { get; } = sessionName;
}