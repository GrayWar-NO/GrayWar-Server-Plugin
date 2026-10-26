using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Gives instructions on how to join the discord server
/// </summary>
[AutoCommand]
public class DiscordCommand: ConfigurableCommand, IGameCommand
{
    /// <summary>
    ///     Default builder for <see cref="DiscordCommand"/>
    /// </summary>
    /// <param name="config"></param>
    public DiscordCommand(ConfigFile config) : base(config)
    {
        _joinCode = config.Bind("Discord Command", "JoinCode", "zfMMZD4kHE");
        _url = config.Bind("Discord Command", "URL", "graywar.no");
    }
    
    private readonly ConfigEntry<string> _joinCode;
    private readonly ConfigEntry<string> _url;
    
    /// <inheritdoc />
    public override string Name => "discord";

    /// <inheritdoc />
    public override string Description => "Get instructions on how to join the discord server.";
    
    /// <inheritdoc />
    public UniTask<ChatCommandResult> Execute(ChatCommandRequest request)
    {
        return UniTask.FromResult(ChatCommandResult.Ok($"Discord join code: {_joinCode.Value} \nor go to {_url.Value}"));
    }
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}