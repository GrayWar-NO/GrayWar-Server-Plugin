using System.Globalization;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
using GW_server_plugin.Features.IPC.Packets;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Donate a specified sum in millions to a player
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class DonateCommand(ConfigFile config): PermissionConfigurableCommand(config), IGameCommand
{

    /// <inheritdoc />
    public override string Name => "donate";

    /// <inheritdoc />
    public override string Description => "Donate your own money to a teammate.";

    /// <inheritdoc />
    public override string Usage => "/donate <target / targetID> <sum in millions (eg. 10 = 10 million)>";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => UniTask.FromResult(args.Length == 2);
    
    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        var found = PlayerUtils.TryFindPlayer(args[0], out var targetPlayer);
        if (!found || targetPlayer == null)
        {
            return UniTask.FromResult<(bool, string?)>((false, $"Could not find a player by {args[0]}"));
        }
        
        if (player == targetPlayer)
        {
            return UniTask.FromResult<(bool, string?)>((false, "You can not donate to yourself."));
        }
        
        var amountText = args[1].Trim();

        if (amountText.Contains(".") && amountText.Contains(","))
        {
            return UniTask.FromResult<(bool, string?)>((false, "Use either ',' or '.' as the decimal separator, not both."));
        }

        amountText = amountText.Replace(',', '.');

        if (!decimal.TryParse(
                amountText,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out var amount) || float.IsNaN((float) amount))
        {
            return UniTask.FromResult<(bool, string?)>((false, $"Could not parse '{args[1]}' as a number."));
        }

        if (amount <= 0m)
        {
            return UniTask.FromResult<(bool, string?)>((false, "Sum must be a positive number."));
        }

        var sum = (float)amount;

        if (player.Allocation < sum)
        {
            return UniTask.FromResult<(bool, string?)>((false,
                $"Insufficient allocation. You tried to donate {sum} (million), but only have {player.Allocation} (million) available."));
        }
        
        if (player.HQ != targetPlayer.HQ)
            return UniTask.FromResult<(bool, string?)>((false, "You can only donate to players in the same faction."));
        
        // Deduct from player and give to target
        player.AddAllocation(-sum);
        targetPlayer.AddAllocation(sum);
        
        ChatService.SendPrivateChatMessage($"{player.PlayerName} has given you {sum} (million)!", targetPlayer);
        
        // Logging
        var donatePacket = new LogEntryPacket
        {
            Channel = LogChannel.Donate,
            LogText = $"{player.SteamID}:{targetPlayer.SteamID}:{sum}"
        };
        GwServerPlugin.LoggingOutBox.Add(donatePacket);
        return UniTask.FromResult<(bool, string?)>((true, $"You have successfully donated {sum} (million) to {targetPlayer.PlayerName}."));
    }
    
    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
}