using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to link a steam user to their discord profile.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class LinkmeCommand(ConfigFile config): PermissionConfigurableCommand(config), IGameCommand
{

    private HashSet<int> _usedCodes = []; 
    private Random _rnd = new();
    
    /// <inheritdoc />
    public override string Name => "linkme";

    /// <inheritdoc />
    public override string Description => "Use this to link your steam account with your discord in our system";

    /// <inheritdoc />
    public override string Usage => "/linkme (takes no arguments)";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => UniTask.FromResult(args.Length == 0);
    
    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        var steamID = player.SteamID;
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
        return UniTask.FromResult<(bool, string?)>((true, $"Your code is {code} . Use /linkme <CODE> in discord to link your accounts. Valid 10 minutes."));
    }
    
    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}