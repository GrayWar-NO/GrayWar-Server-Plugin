using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using GW_server_plugin.Helpers;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
///     Allows users to report sus behaviour to server staff. 
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class ReportCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand
{
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
    
    /// <inheritdoc />
    public override string Name => "report";
    
    /// <inheritdoc />
    public override string Description => "Reports something to staff";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("reason", ChatArgumentType.Text)
    ];
    
    /// <inheritdoc />
    public ChatCommandResult Execute(ChatCommandRequest request)
    {
        GwServerPlugin.GrpcMgr.Client?.sendReportAsync(new ServerReport
        {
            Content = request.Args[0],
            Username = request.Sender.player.GetLogName()
        });
        return ChatCommandResult.Ok($"Reported {request.Args[0]} to staff successfully!");
    }
}
