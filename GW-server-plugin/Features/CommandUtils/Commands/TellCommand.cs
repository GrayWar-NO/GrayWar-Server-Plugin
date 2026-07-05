using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Tells something to everyone on the server
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class TellCommand(ConfigFile config) : PermissionConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string Name => "tell";

    /// <inheritdoc />
    public override string Description => "Broadcast a message to everyone on the server.";

    /// <inheritdoc />
    public override string Usage => "tell <message>";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(args.Length > 0);
    }

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Execute(args);

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var message = string.Join(" ", args);
        ChatService.SendChatMessageAsServer(message);
        return new UniTask<(bool success, string? response)>((true, null));
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}