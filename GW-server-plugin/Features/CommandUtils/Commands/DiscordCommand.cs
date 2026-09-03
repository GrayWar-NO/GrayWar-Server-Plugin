using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using NuclearOption.Networking;

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
    public override string OutputName => "discord";

    /// <inheritdoc />
    public override string Description => "Get instructions on how to join the discord server.";

    /// <inheritdoc />
    public override string Usage => "discord (takes no arguments)";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => UniTask.FromResult(args.Length == 0);

    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        return UniTask.FromResult<(bool, string?)>((true, $"Discord join code: {_joinCode.Value} \nor go to {_url.Value}"));
    }
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}