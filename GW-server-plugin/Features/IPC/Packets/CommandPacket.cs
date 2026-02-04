namespace GW_server_plugin.Features.IPC.Packets;

public class CommandPacket: CommunicationPacket
{ 
    public string CommandName { get; set; }
    public string[] Parameters { get; set; }

    /// <summary>
    /// Process method for Command packet.
    /// orders running the command in question
    /// </summary>
    public override CommunicationPacket? Process()
    {
        GwServerPlugin.Logger?.LogDebug($"command {CommandName} recieved with args: {string.Join("; ", Parameters)}");
        //todo Return Commands.Execute() something something
        return null;

    }

}