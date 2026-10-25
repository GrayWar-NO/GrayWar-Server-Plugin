using System;
using NuclearOption.ModdingApi;

namespace GW_server_plugin.Features.CommandUtils;

public static class InterfaceMethods
{
    public static void RegisterCommandAsServerCommand(IGameCommand command)
    {
        ChatCommands.RegisterServerCommand(
            command.Name,
            command.Description,
            command.Execute,
            command.Arguments,
            ProtoToShockfrontPermissionLevel(command.PermissionLevel),
            command.Aliases
        );
    }
    
    private static PermissionLevel ProtoToShockfrontPermissionLevel(
        Com.Graywar.NoServerManager.Proto.PermissionLevel permissionLevel)
    {
        switch (permissionLevel)
        {
            case Com.Graywar.NoServerManager.Proto.PermissionLevel.Owner:
                return PermissionLevel.Host;
            case Com.Graywar.NoServerManager.Proto.PermissionLevel.Admin:
                return PermissionLevel.Admin;
            case Com.Graywar.NoServerManager.Proto.PermissionLevel.Moderator:
                return PermissionLevel.Moderator;
            case Com.Graywar.NoServerManager.Proto.PermissionLevel.Everyone:
            default:
                return PermissionLevel.Player;
        }
    }
    
}