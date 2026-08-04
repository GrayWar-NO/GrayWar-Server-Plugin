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
    ///     List of active inhibitors for a vote to start.
    /// </summary>
    private static readonly HashSet<string> Inhibitors = [];
    
    /// <summary>
    ///     Currently in use votesession or null
    /// </summary>
    public static IVoteSession? Session;
    
    /// <summary>
    ///     VoteSession factories that exist.
    /// </summary>
    public static readonly Dictionary<string, Func<Player, string?, IVoteSession>> Factories = new();
    
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
            
            MethodInfo? initMethod = null;
            for (var type = sessionType.BaseType; type != null; type = type.BaseType)
            {
                initMethod = type.GetMethod(
                    "InitializeConfig",
                    BindingFlags.NonPublic | BindingFlags.Static);
                
                if (initMethod != null)
                    break;
            }
            
            if (initMethod == null)
            {
                GwServerPlugin.Logger.LogWarning($"Failed to initialize vote session for {sessionType.Name}: InitializeConfig method not found");
                continue;
            }
            
            initMethod.Invoke(null, [pluginConfig, categoryName]);
            Factories[sessionAttr.ShortName] = (p, s) => (IVoteSession)Activator.CreateInstance(sessionType, p, s);
            GwServerPlugin.Logger.LogInfo($"Initialized vote session for {sessionType.Name} successfully");
        }
    }    
    /// <summary>
    ///     Starts a voteSession.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    public static bool TryStartVote(IVoteSession session, out string? response)
    {
        response = null;
        if (Inhibitors.Any())
        {
            response = $"Cannot start vote: inhibited:\n{string.Join("\n", Inhibitors)}";
            return false;
        }
        
        if (Session != null)
        {
            response = $"Cannot start vote: there is already a {Session.SessionName} vote ongoing.";
            return false;
        }
        
        Session = session;
        Session.Start();
        response = $"{Session.SessionName} vote started! Dont forget to use /vote to vote for the outcome you want!";
        return true;
    }
    
    /// <summary>
    ///     Destroys the current voteSession.
    /// </summary>
    public static void CancelVote()
    {
        Session?.Destroy();
        Session = null;
    }
    
    /// <summary>
    ///     Adds an inhibition reason to the voting. Also cancels any existing vote if one is active.
    /// </summary>
    /// <param name="reason"></param>
    public static void Inhibit(string reason)
    {
        if (!Inhibitors.Any()) CancelVote();
        Inhibitors.Add(reason);
    }
    
    /// <summary>
    ///     Removes an inhibitor from the current inhibition reasons.
    /// </summary>
    /// <param name="reason"></param>
    public static bool RemoveInhibit(string reason) => Inhibitors.Remove(reason);
}

/// <summary>
///     Attribute to mark a voteSession as implicitly used by the Reflection discovery in the VoteManager class.
/// </summary>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class)]
public sealed class AutoVoteSessionAttribute(string sessionName, string shortName) : Attribute
{
    /// <summary>
    ///     ShortName for the VoteSession
    /// </summary>
    public string ShortName { get; } = shortName;
    
    /// <summary>
    ///     SessionName for the voteSession
    /// </summary>
    public string SessionName { get; } = sessionName;
}