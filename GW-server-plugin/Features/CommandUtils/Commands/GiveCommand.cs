using System.Globalization;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
/// Donate a specified sum in millions to a player
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class GiveCommand(ConfigFile config): PermissionConfigurableCommand(config), IGameCommand, IConsoleCommand
{

    /// <inheritdoc />
    public override string Name => "give";

    /// <inheritdoc />
    public override string Description => "Give money to someone out of thin air";

    /// <inheritdoc />
    public override string Usage => "/give <target / targetID> <sum in millions (eg. 10 = 10 million)>";

    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args) => UniTask.FromResult(args.Length == 2);

    /// <inheritdoc />
    public UniTask<bool> Validate(string[] args) => UniTask.FromResult(args.Length == 2);


    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args) => Execute(args);
    
    
    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(string[] args)
    {
        var found = PlayerUtils.TryFindPlayer(args[0], out var targetPlayer);
        if (!found || targetPlayer == null)
        {
            return UniTask.FromResult<(bool, string?)>((false, $"Could not find a player by {args[0]}"));
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
        targetPlayer.AddAllocation(sum);
        
        ChatService.SendPrivateChatMessage($"you were given you {sum} (million)!", targetPlayer);
        return UniTask.FromResult<(bool, string?)>((true, $"You have successfully given {sum}m to {targetPlayer.PlayerName}."));
    }

    /// <inheritdoc />
    public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;

}