using System.Globalization;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.ModdingApi;
using NuclearOption.Networking;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands.Utils;

/// <summary>
/// Donate a specified sum in millions to a player
/// </summary>
/// <param name="config"></param>
[AutoCommand]
public class GiveCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand, IConsoleCommand
{
    /// <inheritdoc />
    public override string Name => "give";
    
    /// <inheritdoc />
    public override string Description => "Give money to someone out of thin air";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("target", ChatArgumentType.Player),
        new("amountMillions", ChatArgumentType.Float)
    ];
    
    /// <inheritdoc />
    public async UniTask<ChatCommandResult> Execute(ChatCommandRequest request)
    {
        return !request.TryGetTargetPlayer(0, out var targetPlayer, out var error)
            ? ChatCommandResult.ClientError(error)
            : await Common(request.Args[1], targetPlayer.player);
    }
    
    
    /// <inheritdoc />
    public async UniTask<ChatCommandResult> Execute(string[] args)
    {
        var found = PlayerUtils.TryFindPlayer(args[0], out var targetPlayer);
        if (!found || targetPlayer == null)
        {
            return ChatCommandResult.ClientError($"Could not find a player by {args[0]}");
        }
        
        return await Common(args[1], targetPlayer);
    }
    
    private static UniTask<ChatCommandResult> Common(string amountText, Player targetPlayer)
    {
        if (amountText.Contains(".") && amountText.Contains(","))
        {
            return UniTask.FromResult(ChatCommandResult.ClientError("Use either ',' or '.' as the decimal separator, not both."));
        }
        
        amountText = amountText.Replace(',', '.');
        
        if (!decimal.TryParse(
                amountText,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out var amount) || float.IsNaN((float)amount))
        {
            return UniTask.FromResult(ChatCommandResult.ClientError($"Could not parse '{amountText}' as a number."));
        }
        
        var sum = (float)amount;
        targetPlayer.AddAllocation(sum);
        
        ChatService.SendPrivateChatMessage($"You were given {sum} (million)!", targetPlayer);
        return UniTask.FromResult(ChatCommandResult.Ok(
            $"You have successfully given {sum} (million) to {targetPlayer.GetDisplayName()}."));
    }
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Moderator;
}