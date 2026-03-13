using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Features.IPC.Packets;
using Newtonsoft.Json;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to link a steam user to their discord profile.
/// </summary>
/// <param name="config"></param>
public class LinkmeCommand(ConfigFile config): PermissionConfigurableCommand(config)
{

    private HashSet<int> _usedCodes = []; 
    private Random _rnd = new Random();
    
    /// <inheritdoc />
    public override string Name => "linkme";

    /// <inheritdoc />
    public override string Description => "Use this to link your steam account with your discord in our system";

    /// <inheritdoc />
    public override string Usage => "/linkme (takes no arguments)";

    /// <inheritdoc />
    public override bool Validate(Player player, string[] args) => args.Length == 0;

    /// <inheritdoc />
    public override bool Validate(string[] args) => true; // not valid if from console

    /// <inheritdoc />
    public override bool Execute(Player player, string[] args, out string? response)
    {
        var steamID = player.SteamID;
        var code = _rnd.Next(1000000);
        while (_usedCodes.Contains(code))
        {
            code = _rnd.Next(1000000);
        }
        _usedCodes.Add(code);
        response = $"Your code is {code} . Use /linkme <CODE> in discord to link your accounts. Valid 10 minutes.";
        var packet = new LinkPacket
        {
            SteamID = steamID,
            OneTimeCode = code
        };
        GwServerPlugin.SocketOutBox.Add(JsonConvert.SerializeObject(packet));
        return true;
    }

    /// <inheritdoc />
    public override bool Execute(string[] args, out string? response)
    {
        response = "This command can not be called from the console and has therefore no effect.";
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}