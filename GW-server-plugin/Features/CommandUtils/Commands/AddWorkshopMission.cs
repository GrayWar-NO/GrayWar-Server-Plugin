using System;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
using GW_server_plugin.Helpers;
using GW_server_plugin.Patches;
using NuclearOption.DedicatedServer;
using NuclearOption.Networking;
using NuclearOption.Workshop;
using Steamworks;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Command to add a workshop mission to the server.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class AddWorkshopMission(ConfigFile config): PermissionConfigurableCommand(config), IConsoleCommand, IGameCommand
{
    /// <inheritdoc />
    public override string Name => "addmission";


    /// <inheritdoc />
    public override string Description => "Adds a mission to the server from it's workshopID.\nThis has temporary effect and won't be persisted after a server restart.";

    /// <inheritdoc />
    public override string Usage => "addmission <workshopID>";

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args)
    {
        return UniTask.FromResult(args.Length == 1 && ulong.TryParse(args[0], out _));
    }
    
    /// <inheritdoc />
    public async UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var keySaveable = new MissionKeySaveable
        {
            Group = "Workshop",
            Name = args[0],
        };
        
        var workshopID = ulong.Parse(args[0]);
        try
        {
            var downloadResult = await SteamWorkshop.DownloadItemServer(new PublishedFileId_t(workshopID));
            if (!downloadResult) return (false, "Failed to download workshop item");
        }
        catch (Exception ex)
        {
            return (false, $"Failed to download workshop item: {ex.Message}");
        }
        if (!keySaveable.TryGetKey(out var key)) return (false, $"{keySaveable.Name} is not a valid key");
        key = MissionNameFix.TranslateWorkshopName(key);

        MissionService.AddMission(new MissionOptions{Key = keySaveable, MaxTime = 14400f});
        
        return (true, $"Added mission {key.Name} to rotation successfully.");
    }

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => Validate(args);

    /// <inheritdoc />
    public async UniTask<(bool success, string? response)> Execute(Player player, string[] args) => await Execute(args);

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}