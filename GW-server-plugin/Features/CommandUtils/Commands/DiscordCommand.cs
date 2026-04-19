using BepInEx.Configuration;
using GW_server_plugin.Enums;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Gives instructions on how to join the discord server
/// </summary>
/// <param name="config"></param>
public class DiscordCommand(ConfigFile config): PermissionConfigurableCommand(config)
{

    /// <inheritdoc />
    public override string Name => "discord";

    /// <inheritdoc />
    public override string Description => "Get instructions on how to join the discord server.";

    /// <inheritdoc />
    public override string Usage => "/discord (takes no arguments)";

    /// <inheritdoc />
    public override bool Validate(Player player, string[] args) => args.Length == 0;

    /// <inheritdoc />
    public override bool Validate(string[] args) => true; // not valid if from console

    /// <inheritdoc />
    public override bool Execute(Player player, string[] args, out string? response)
    {
        response = $"Discord join code: zfMMZD4kHE \nor go to graywar.no";
        return true;
    }

    /// <inheritdoc />
    public override bool Execute(string[] args, out string? response)
    {
        response = "This command can not be called from the console and has therefore no effect.";
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}