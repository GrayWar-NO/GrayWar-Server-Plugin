using System;
using BepInEx.Configuration;
using Com.Graywar.NoServerManager.Proto;
using Cysharp.Threading.Tasks;
using NuclearOption.Networking;

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
    public override string OutputName => "gamble";
    
    /// <inheritdoc />
    public override string Description =>
        "50% chance you receive 2x your bet. IF you lose, 20% chance you eject out of rage";
    
    /// <inheritdoc />
    public override string Usage =>
        $"gamble <$ in million>. eg: '{PluginConfig.CommandPrefixChar}gamble 100' gambles 100m";
    
    /// <inheritdoc />
    protected override PermissionLevel DefaultPermissionLevel { get; } = PermissionLevel.Everyone;
    
    /// <inheritdoc />
    public UniTask<bool> Validate(Player player, string[] args)
    {
        if (!int.TryParse(args[0], out _))
        {
            return UniTask.FromResult(false);
        }
        
        if (int.Parse(args[0]) <= 0)
            return UniTask.FromResult(false);
        
        
        return UniTask.FromResult(true);
    }
    
    /// <inheritdoc />
    public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
    {
        if (int.Parse(args[0]) > player.Allocation)
        {
            var r = "You don't have that much money. Please try again";
            return UniTask.FromResult<(bool success, string? response)>((false, r));
        }
        
        if (player.Aircraft == null)
        {
            var r = "You must be spawned in before you can gamble!";
            return UniTask.FromResult<(bool success, string? response)>((false, r));
        }
        
        if (!player.Aircraft.airborne)
        {
            var r = "You must be airborne before you can gamble!";
            return UniTask.FromResult<(bool success, string? response)>((false, r));
        }
        
        var bet = int.Parse(args[0]);
        if (GambleService.CoinFlip())
        {
            ChatService.SendChatMessageAsServer($"{player.SteamID} has won +${bet}m!");
            player.SetAllocation(player.Allocation + bet);
            ChatService.SendPrivateChatMessage($"Current balance: ${player.Allocation}m", player);
        }
        else
        {
            ChatService.SendChatMessageAsServer($"{player.SteamID} has lost -${bet}m!", player);
            player.SetAllocation(player.Allocation - bet);
            var punishment = GambleService.Rnd.NextDouble() * 100;
            if (punishment <= 20)
            {
                ChatService.SendChatMessageAsServer($"{player.SteamID} has ejected out of rage!", player);
                player.Aircraft.StartEjectionSequence();
            }
            
            ChatService.SendPrivateChatMessage($"Current balance: ${player.Allocation}m", player);
        }
        
        return UniTask.FromResult<(bool success, string? response)>((true, null));
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
    public static readonly Random Rnd = new Random();
    
    /// <summary>
    /// 50% chance it returns either 0 and 1
    /// </summary>
    /// <returns>0 or 1</returns>
    public static bool CoinFlip()
    {
        return Rnd.Next(0, 2) == 0;
    }
}