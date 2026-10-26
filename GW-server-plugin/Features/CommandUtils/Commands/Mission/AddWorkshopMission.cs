using System;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using NuclearOption.DedicatedServer;
using NuclearOption.ModdingApi;
using NuclearOption.Workshop;
using Steamworks;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Mission;

/// <summary>
/// Command to add a workshop mission to the server.
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class AddWorkshopMission(ConfigFile config) : CommonBehaviourCommand(config)
{
    /// <inheritdoc />
    public override string Name => "addmission";
    
    
    /// <inheritdoc />
    public override string Description =>
        "Adds a mission to the server from it's workshopID.\nThis has temporary effect and won't be persisted after a server restart.";
    
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("WorkshopID", ChatArgumentType.Text),
        new("save", ChatArgumentType.Text, true)
    ];
    
    /// <inheritdoc />
    public override async UniTask<ChatCommandResult> Execute(string[] args)
    {
        var keySaveable = new MissionKeySaveable
        {
            Group = "Workshop",
            Name = args[0],
        };
        
        var workshopID = ulong.Parse(args[0]);
        // ReSharper disable once SimplifyConditionalTernaryExpression
        var save = args.Length > 1 ? bool.Parse(args[1]) : false;
        try
        {
            var downloadResult = await SteamWorkshop.DownloadItemServer(new PublishedFileId_t(workshopID));
            if (!downloadResult) return ChatCommandResult.ClientError("Failed to download workshop item");
        }
        catch (Exception ex)
        {
            return ChatCommandResult.Error(1, $"Failed to download workshop item: {ex.Message}");
        }
        
        if (!keySaveable.TryGetKey(out var key)) return ChatCommandResult.ClientError($"{keySaveable.Name} is not a valid workshopID");
        
        MissionService.AddMission(new MissionOptions { Key = keySaveable, MaxTime = 14400f }, save);
        
        return ChatCommandResult.Ok($"Added mission {key.Name} to rotation successfully.");
    }
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}