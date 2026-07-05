using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Gives instructions on how to join the discord server
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class DiscordCommand(ConfigFile config): PermissionConfigurableCommand(config), IGameCommand
{

    /// <inheritdoc />
    public override string Name => "discord";

    /// <inheritdoc />
    public override string Description => "Get instructions on how to join the discord server.";

    /// <inheritdoc />
    public override string Usage => "/discord (takes no arguments)";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => UniTask.FromResult(args.Length == 0);

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        return UniTask.FromResult<(bool, string?)>((true, "Discord join code: zfMMZD4kHE \nor go to graywar.no"));
    }
    
    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}