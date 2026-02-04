namespace GW_server_plugin.Features.IPC.Packets;

public class ResponsePacket: CommunicationPacket
{
    public string ResponseText { get; set; }
    
    public override CommunicationPacket? Process()
    {
        GwServerPlugin.Logger?.LogWarning("Tried to process an outgoing only response packet.");
        return null;
    }
}