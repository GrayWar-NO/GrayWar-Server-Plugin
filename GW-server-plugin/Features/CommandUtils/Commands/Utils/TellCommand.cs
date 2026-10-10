using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;

namespace GW_server_plugin.Features.CommandUtils.Commands.Utils;

/// <summary>
/// Tells something to everyone on the server
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class TellCommand(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string OutputName => "tell";

    /// <inheritdoc />
    public override string Description => "Broadcast a message to everyone on the server.";

    /// <inheritdoc />
    public override string Usage => "tell <message>";
    
    /// <inheritdoc />
    public override UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(args.Length > 0);
    }
    
    /// <inheritdoc />
    public override UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var message = string.Join(" ", args);
        ChatService.SendChatMessageAsServer(message);
        return new UniTask<(bool success, string? response)>((true, null));
    }

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}