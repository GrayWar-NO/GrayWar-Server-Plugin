using System;
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
    public abstract string Name { get; }

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
    
    
    private ConfigEntry<bool> EnableConfig { get; }

    /// <summary>
    ///     The default permission level required to execute the command.
    /// </summary>
    public abstract PermissionLevel DefaultPermissionLevel { get; }
    
    /// <summary>
    ///     Default value for the enable toggle of this command.
    /// </summary>
    public virtual bool DefaultEnable => true;
    
    /// <summary>
    ///     Constructor for the base command.
    /// </summary>
    /// <param name="config"> BepInEx configuration file. </param>
    protected ConfigurableCommand(ConfigFile config)
    {
        // ReSharper disable VirtualMemberCallInConstructor
        EnableConfig = config.Bind(CommandConfigSection, $"Enable {Name}", DefaultEnable, $"Enable toggle for {Name}");
        PermissionLevelConfig = config.Bind(CommandConfigSection, Name, DefaultPermissionLevel, $"Permission level for command {Name}");
        // ReShaper restore VirtualMemberCallInConstructor
    }
}

/// <summary>
/// Attribute to mark a command as implicitly used by the Reflection discovery in the base plugin class.
/// </summary>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class)]
public sealed class AutoCommandAttribute : Attribute;