using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Utils;

/// <summary>
/// Tells something to everyone on the server
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class TellCommand(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string Name => "tell";

    /// <inheritdoc />
    public override string Description => "Broadcast a message to everyone on the server.";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("message", ChatArgumentType.Text)
    ];
    
    /// <inheritdoc />
    public override UniTask<ChatCommandResult> Execute(string[] args)
    {
        var message = string.Join(" ", args);
        ChatUtils.SendChatMessageAsServer(message);
        return UniTask.FromResult(ChatCommandResult.Ok("Message sent!"));
    }

    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}