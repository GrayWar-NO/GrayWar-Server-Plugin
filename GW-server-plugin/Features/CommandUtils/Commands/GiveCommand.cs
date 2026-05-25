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
public class GiveCommand(ConfigFile config): PermissionConfigurableCommand(config), IGameCommand, IConsoleCommand
{

    /// <inheritdoc />
    public override string Name => "give";

    /// <inheritdoc />
    public override string Description => "Give money to someone out of thin air";

    /// <inheritdoc />
    public override string Usage => "/give <target / targetID> <sum in millions (eg. 10 = 10 million)>";

    /// <inheritdoc />
    public bool Validate(Player player, string[] args) => args.Length == 2;

    /// <inheritdoc />
    public bool Validate(string[] args) => args.Length == 2;


    /// <inheritdoc />
    public bool Execute(Player player, string[] args, out string? response) => Execute(args, out response);
    
    
    /// <inheritdoc />
    public bool Execute(string[] args, out string? response)
    {
        var found = PlayerUtils.TryFindPlayer(args[0], out var targetPlayer);
        if (!found || targetPlayer == null)
        {
            response = $"Could not find a player by {args[0]}";
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
        targetPlayer.AddAllocation(sum);
        
        response = $"You have successfully given {sum}m to {targetPlayer.PlayerName}.";
        ChatService.SendPrivateChatMessage($"you were given you {sum} (million)!", targetPlayer);
        return true;
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;

}