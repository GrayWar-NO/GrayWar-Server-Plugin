using System;
using System.Globalization;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Google.Protobuf.WellKnownTypes;
using GW_server_plugin.Helpers;
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
        new("amount (in millions)", ChatArgumentType.Text)
    ];
    
    /// <inheritdoc />
    public ChatCommandResult Execute(ChatCommandRequest request)
    {
        var args = request.Args;
        var player = request.Sender.player;
        
        if (!request.TryGetTargetPlayer(0, out var target, out var error))
        {
            return ChatCommandResult.ClientError(error);
        }
        
        if (player == target.player)
        {
            return ChatCommandResult.ClientError("You can not donate to yourself.");
        }
        
        var amountText = args[1].Trim();

        if (amountText.Contains(".") && amountText.Contains(","))
        {
            return ChatCommandResult.ClientError("Use either ',' or '.' as the decimal separator, not both.");
        }

        amountText = amountText.Replace(',', '.');

        if (!decimal.TryParse(
                amountText,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out var amount) || float.IsNaN((float) amount))
        {
            return ChatCommandResult.ClientError($"Could not parse '{args[1]}' as a number.");
        }

        if (amount <= 0m)
        {
            return ChatCommandResult.ClientError("Sum must be a positive number.");
        }

        var sum = (float)amount;

        if (player.Allocation < sum)
        {
            return ChatCommandResult.ClientError($"Insufficient allocation. You tried to donate {sum} (million), but only have {player.Allocation} (million) available.");
        }
        
        if (player.HQ != target.player.HQ)
            return ChatCommandResult.ClientError("You can only donate to players in the same faction.");
        
        // Deduct from player and give to target
        player.AddAllocation(-sum);
        target.player.AddAllocation(sum);
        
        ChatService.SendPrivateChatMessage($"{player.GetColoredDisplayName()} has given you {sum} (million)!", target.player);
        
        // Logging
        var log = new DonationLog
        {
            AmountMillions = (uint)sum,
            DonatorSteamID = player.SteamID,
            ReceiverSteamID = target.SteamId,
            Time = DateTime.UtcNow.ToTimestamp()
        };
        GwServerPlugin.GrpcMgr.Client?.sendDonationAsync(log);
        
        return ChatCommandResult.Ok($"You have successfully donated {sum} (million) to {target.player.GetDisplayName()}.");
    }
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}
