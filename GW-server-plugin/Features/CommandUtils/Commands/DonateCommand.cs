using System;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Donate a specified sum in millions to a player
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class DonateCommand(ConfigFile config): ConfigurableCommand(config), IGameCommand
{

    /// <inheritdoc />
    public override string Name => "donate";

    /// <inheritdoc />
    public override string Description => "Donate your own money to a teammate.";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("player", ChatArgumentType.Player),
        new("amountMillions", ChatArgumentType.Float)
    ];
    
    /// <inheritdoc />
    public UniTask<ChatCommandResult> Execute(ChatCommandRequest request)
    {
        if (!request.TryGetTargetPlayer(0, out var target, out var error))
        {
            return UniTask.FromResult(ChatCommandResult.ClientError(error));
        }
        
        if (request.Sender == target)
        {
            return UniTask.FromResult(ChatCommandResult.ClientError("You can not donate to yourself."));
        }
        
        if (!request.TryGetFloat(1, out var amount, out error))
        {
            return UniTask.FromResult(ChatCommandResult.ClientError(error));
        }
        
        if (amount <= 0)
        {
            return UniTask.FromResult(ChatCommandResult.ClientError("Sum must be a positive number."));
        }
        
        var player = request.Sender.player;

        if (player.Allocation < amount)
        {
            return UniTask.FromResult(ChatCommandResult.ClientError($"Insufficient allocation. You tried to donate {amount} (million), but only have {player.Allocation} (million) available."));
        }
        
        if (player.HQ != target.player.HQ)
            return UniTask.FromResult(ChatCommandResult.ClientError("You can only donate to players in the same faction."));
        
        // Deduct from player and give to target
        player.AddAllocation(-amount);
        target.player.AddAllocation(amount);
        
        ChatUtils.SendPrivateChatMessage($"{player.MentionArg} has given you {amount} (million)!", target.player);
        
        // Logging
        var log = new DonationLog
        {
            AmountMillions = (uint)amount,
            DonatorSteamID = player.SteamID,
            ReceiverSteamID = target.SteamId,
            Time = DateTime.UtcNow.ToTimestamp()
        };
        GwServerPlugin.GrpcMgr.Client?.sendDonationAsync(log);
        
        return UniTask.FromResult(ChatCommandResult.Ok($"You have successfully donated {amount} (million) to {target.Mention}."));
    }
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}
