using System;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils;

/// <summary>
///     Base class for commands that can be configured with a permission level.
/// </summary>
public abstract class ConfigurableCommand : ICommand
{
    private string CommandConfigSection => $"{Name} command";
    
    /// <inheritdoc />
    public abstract string Name { get; }
    
    /// <inheritdoc />
    public abstract string Description { get; }
    
    /// <summary>
    ///     default aliases for this command.
    /// </summary>
    protected virtual string[] DefaultAliases => [];
    
    /// <inheritdoc />
    public virtual ChatCommandArgument[] Arguments => [];
    
    /// <inheritdoc />
    public PermissionLevel PermissionLevel => PermissionLevelConfig.Value;
    
    /// <inheritdoc />
    public string[] Aliases => AliasesConfig.Value.Split(';');
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool MatchesNameOrAlias(string name)
    {
        return name.Equals(Name, StringComparison.OrdinalIgnoreCase) ||
               Aliases.Any(alias => name.Equals(alias, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    ///     Getter for the enable config option.
    /// </summary>
    public bool Enable => EnableConfig.Value;
    
    /// <summary>
    ///     The command permission level configuration.
    /// </summary>
    private ConfigEntry<PermissionLevel> PermissionLevelConfig { get; }
    
    private ConfigEntry<string> AliasesConfig { get; }
    
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
        EnableConfig = config.Bind(CommandConfigSection, "Enable", DefaultEnable,
            $"Enable toggle for {Name}");
        PermissionLevelConfig = config.Bind(CommandConfigSection, "Permission level", DefaultPermissionLevel,
            $"Permission level for command {Name}");
        AliasesConfig = config.Bind(CommandConfigSection, "Aliases", string.Join(";", DefaultAliases),
            "Alias names for this command. ; separated value list.");
        // ReShaper restore VirtualMemberCallInConstructor
    }
}

/// <summary>
/// Attribute to mark a command as implicitly used by the Reflection discovery in the base plugin class.
/// </summary>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class)]
public sealed class AutoCommandAttribute : Attribute;

/// <summary>
/// Attribute to mark a command as Dynamic. Dynamic commands will not be instantiated or registered automatically, and must rely on other mechanisms to do so.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DynamicCommandAttribute : Attribute;