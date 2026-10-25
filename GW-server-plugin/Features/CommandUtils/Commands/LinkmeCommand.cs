using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to link a steam user to their discord profile.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class LinkmeCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand
{
    private readonly HashSet<int> _usedCodes = [];
    private readonly Random _rnd = new();
    
    /// <inheritdoc />
    public override string Name => "linkme";
    
    /// <inheritdoc />
    public override string Description => "Use this to link your steam account with your discord in our system";
    
    /// <inheritdoc />
    public ChatCommandResult Execute(ChatCommandRequest request)
    {
        var steamID = request.Sender.SteamId;
        var code = _rnd.Next(1000000);
        while (_usedCodes.Contains(code))
        {
            code = _rnd.Next(1000000);
        }
        
        _usedCodes.Add(code);
        
        var log = new LinkUser
        {
            OneTimeCode = (uint)code,
            SenderSteamID = steamID,
        };
        _ = GwServerPlugin.GrpcMgr.Client?.sendLinkCodeAsync(log);
        return ChatCommandResult.Ok(
            $"Your code is {code} . Use /linkme <CODE> in discord to link your accounts. Valid 10 minutes.");
    }
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}