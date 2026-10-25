using System;
using BepInEx.Configuration;
using GW_server_plugin.Helpers;
using NuclearOption.ModdingApi;
using PermissionLevel = Com.Graywar.NoServerManager.Proto.PermissionLevel;

namespace GW_server_plugin.Features.CommandUtils.Commands;

/// <summary>
///   Gamble
/// </summary>
/// 
[AutoCommand]
public class GambleCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand
{
    /// <inheritdoc />
    protected override bool DefaultEnable => false;
    
    /// <inheritdoc />
    public override string Name => "gamble";
    
    /// <inheritdoc />
    public override string Description =>
        "50% chance you receive 2x your bet. IF you lose, 20% chance you eject out of rage";
    
    /// <inheritdoc />
    public override ChatCommandArgument[] Arguments =>
    [
        new("amount (in millions)", ChatArgumentType.Text)
    ];
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
    
    /// <inheritdoc />
    public ChatCommandResult Execute(ChatCommandRequest request)
    {
        var args = request.Args;
        var player = request.Sender.player;
        if (int.Parse(args[0]) > player.Allocation)
            return ChatCommandResult.Error(0, "You don't have that much money. Please try again");
        
        if (player.Aircraft == null)
            return ChatCommandResult.Error(0, "You must be spawned in before you can gamble!");
        
        if (!player.Aircraft.airborne)
            return ChatCommandResult.Error(0, "You must be airborne before you can gamble!");
        
        var bet = int.Parse(args[0]);
        if (GambleService.CoinFlip())
        {
            ChatService.SendChatMessageAsServer($"{player.GetDisplayName()} has won +${bet}m!");
            player.SetAllocation(player.Allocation + bet);
            ChatService.SendPrivateChatMessage($"Current balance: ${player.Allocation}m", player);
        }
        else
        {
            ChatService.SendChatMessageAsServer($"{player.GetDisplayName()} has lost -${bet}m!", player);
            player.SetAllocation(player.Allocation - bet);
            var punishment = GambleService.Rnd.NextDouble() * 100;
            if (punishment <= 20)
            {
                ChatService.SendChatMessageAsServer($"{player.GetDisplayName()} has ejected out of rage!", player);
                player.Aircraft.StartEjectionSequence();
            }
            
            ChatService.SendPrivateChatMessage($"Current balance: ${player.Allocation}m", player);
        }
        
        return ChatCommandResult.Ok();
    }
}

/// <summary>
/// Helper for gamble
/// </summary>
public static class GambleService
{
    /// <summary>
    /// Randomizer
    /// </summary>
    public static readonly Random Rnd = new();
    
    /// <summary>
    /// 50% chance it returns either 0 and 1
    /// </summary>
    /// <returns>0 or 1</returns>
    public static bool CoinFlip()
    {
        return Rnd.Next(0, 2) == 0;
    }
}