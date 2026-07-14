using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
///     Allows users to report sus behaviour to server staff. 
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class ReportCommand(ConfigFile config) : PermissionConfigurableCommand(config), IGameCommand
{
    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
    
    /// <inheritdoc />
    public override string Name => "report";
    
    /// <inheritdoc />
    public override string Description => "Reports something to graywar staff";
    
    /// <inheritdoc />
    public override string Usage => "report <reason>";
    
    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => UniTask.FromResult(args.Length != 0);
    
    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        var content = string.Join(" ", args);
        GwServerPlugin.GrpcMgr.Client?.sendReportAsync(new serverReport
        {
            Content = content,
            Username = player.PlayerName
        });
        
        return UniTask.FromResult((true, (string?)$"{content} reported to GrayWar staff successfully."));
    }
}