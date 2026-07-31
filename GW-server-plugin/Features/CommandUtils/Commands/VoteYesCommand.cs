// using BepInEx.Configuration;
// using Com.Graywar.NoServerManager.Proto;
// using Cysharp.Threading.Tasks;
// using NuclearOption.Networking;
//
// namespace GW_server_plugin.Features.CommandUtils.Commands;
//
// /// <summary>
// /// Vote YES to the current Generic vote session
// /// </summary>
// /// <param name="config"></param>
// [AutoCommand]
// public class VoteYesCommand(ConfigFile config) : ConfigurableCommand(config), IGameCommand
// {
//     /// <inheritdoc />
//     public override string Name => "y";
//     
//     /// <inheritdoc />
//     public override string Description => "adds a YES vote to the current vote";
//     
//     /// <inheritdoc />
//     public override string Usage  => "y";
//     
//     /// <inheritdoc />
//     public override PermissionLevel DefaultPermissionLevel => PermissionLevel.Everyone;
//     
//     /// <inheritdoc />
//     public UniTask<bool> Validate(Player player, string[] args)
//     {
//         return UniTask.FromResult(true);
//     }
//
//     /// <inheritdoc />
//     public UniTask<(bool success, string? response)> Execute(Player player, string[] args)
//     {
//         if (VoteSession.Instance != null)
//         {
//             VoteSession.Instance.HandleVote(player, true, out var result);
//             return UniTask.FromResult(result);
//         }
//
//         return new UniTask<(bool success, string? response)>((false, "Vote failed, no active vote session"));
//     }
//
// }