using GW_server_plugin.Enums;
using GW_server_plugin.Features.CommandUtils;

namespace GW_server_plugin.Features.IPC.Packets;

/// <summary>
/// Command packet for executing a command on the server
/// </summary>
public class CommandPacket: CommunicationPacket
{
    /// <summary>
    /// The name of the command
    /// </summary>
    public string CommandName { get; set; } = null!;

    /// <inheritdoc />
    public override PacketType Type { get; set; } = PacketType.Command;

    /// <summary>
    /// Arguments for the command
    /// </summary>
    public string[] Arguments { get; set; } = null!;

    /// <summary>
    ///     Whether the called command needs a result.
    /// </summary>
    public bool Result { get; set; } = true;
    
    /// <summary>
    /// Process method for Command packet.
    /// orders running the command in question
    /// </summary>
    public override CommunicationPacket? Process()
    {
        GwServerPlugin.Logger.LogInfo($"command {CommandName} recieved with args: {string.Join("; ", Arguments)}");
        CommandService.TryExecuteCommand(CommandName, Arguments, out var response);
        if (response is null) response = $"Command {CommandName} executed successfully.";
        return Result ? new ResponsePacket { ResponseText = response } : null;
    }
}