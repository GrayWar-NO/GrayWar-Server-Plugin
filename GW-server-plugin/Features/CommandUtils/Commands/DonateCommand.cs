using System.Globalization;
using BepInEx.Configuration;
using GW_server_plugin.Enums;
using GW_server_plugin.Features.IPC.Packets;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Donate a specified sum in millions to a player
/// </summary>
/// <param name="config"></param>
public class DonateCommand(ConfigFile config): PermissionConfigurableCommand(config), IGameCommand
{

    /// <inheritdoc />
    public override string Name => "donate";

    /// <inheritdoc />
    public override string Description => "Donate your own money to a teammate.";

    /// <inheritdoc />
    public override string Usage => "/donate <target / targetID> <sum in millions (eg. 10 = 10 million)>";

    /// <inheritdoc />
    public bool Validate(Player player, string[] args) => args.Length == 2;
    
    /// <inheritdoc />
    public bool Execute(Player player, string[] args, out string? response)
    {
        var found = PlayerUtils.TryFindPlayer(args[0], out var targetPlayer);
        if (!found || targetPlayer == null)
        {
            response = $"Could not find a player by {args[0]}";
            return false;
        }
        
        if (player == targetPlayer)
        {
            response = "You can not donate to yourself.";
            return false;
        }
        
        var amountText = args[1].Trim();

        if (amountText.Contains(".") && amountText.Contains(","))
        {
            response = "Use either ',' or '.' as the decimal separator, not both.";
            return false;
        }

        amountText = amountText.Replace(',', '.');

        if (!decimal.TryParse(
                amountText,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out var amount) || float.IsNaN((float) amount))
        {
            response = $"Could not parse '{args[1]}' as a number.";
            return false;
        }

        if (amount <= 0m)
        {
            response = "Sum must be a positive number.";
            return false;
        }

        var sum = (float)amount;

        if (player.Allocation < sum)
        {
            response = $"Insufficient allocation. You tried to donate {sum} (million), but only have {player.Allocation} (million) available.";
            return false;
        }
        
        if (player.HQ != targetPlayer.HQ)
        {
            response = "You can only donate to players in the same faction.";
            return false;
        }
        
        // Deduct from player and give to target
        player.AddAllocation(-sum);
        targetPlayer.AddAllocation(sum);
        
        response = $"You have successfully donated {sum} (million) to {targetPlayer.PlayerName}.";
        ChatService.SendPrivateChatMessage($"{player.PlayerName} has given you {sum} (million)!", targetPlayer);
        
        // Logging
        var donatePacket = new LogEntryPacket()
        {
            Channel = LogChannel.Donate,
            LogText = $"{player.SteamID}:{targetPlayer.SteamID}:{sum}"
        };
        GwServerPlugin.LoggingOutBox.Add(donatePacket);
        return true;
    }
    
    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}