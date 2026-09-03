using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using JetBrains.Annotations;

namespace GW_server_plugin.Features.CommandUtils;

/// <summary>
///     Base class for commands that can be configured with a permission level.
/// </summary>
public abstract class ConfigurableCommand : ICommand
{
    private const string CommandConfigSection = "Commands";
    
    /// <inheritdoc />
    public IEnumerable<string> Names => Aliases.Value.Append(OutputName);
    
    /// <summary>
    ///     Default alias names for this command.
    /// </summary>
    public virtual IEnumerable<string> DefaultAliases => [];
    
    /// <inheritdoc />
    public abstract string OutputName { get; }
    
    /// <inheritdoc />
    public abstract string Description { get; }
    
    /// <inheritdoc />
    public abstract string Usage { get; }
    
    /// <inheritdoc />
    public PermissionLevel PermissionLevel => PermissionLevelConfig.Value;
    
    /// <summary>
    ///     Getter for the enable config option.
    /// </summary>
    public bool Enable => EnableConfig.Value;
    
    /// <summary>
    ///     The command permission level configuration.
    /// </summary>
    private ConfigEntry<PermissionLevel> PermissionLevelConfig { get; }
    
    private ConfigEntry<string[]> Aliases { get; }
    
    private ConfigEntry<bool> EnableConfig { get; }
    
    /// <summary>
    ///     The default permission level required to execute the command.
    /// </summary>
    protected abstract PermissionLevel DefaultPermissionLevel { get; }
    
    /// <summary>
    ///     Default value for the enable toggle of this command.
    /// </summary>
    protected virtual bool DefaultEnable => true;
    
    /// <summary>
    ///     Constructor for the base command.
    /// </summary>
    /// <param name="config"> BepInEx configuration file. </param>
    protected ConfigurableCommand(ConfigFile config)
    {
        // ReSharper disable VirtualMemberCallInConstructor
        EnableConfig = config.Bind(CommandConfigSection, $"Enable {OutputName}", DefaultEnable,
            $"Enable toggle for {OutputName}");
        PermissionLevelConfig = config.Bind(CommandConfigSection, OutputName, DefaultPermissionLevel,
            $"Permission level for command {OutputName}");
        Aliases = config.Bind(CommandConfigSection, "Aliases", DefaultAliases.ToArray(),
            "Alias names for this command");
        // ReShaper restore VirtualMemberCallInConstructor
    }
}

/// <summary>
/// Attribute to mark a command as implicitly used by the Reflection discovery in the base plugin class.
/// </summary>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class)]
public sealed class AutoCommandAttribute : Attribute;