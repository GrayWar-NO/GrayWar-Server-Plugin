namespace GW_server_plugin.Features.IPC.Packets;

public class LogEntryPacket: CommunicationPacket
{
    public LogChannel Channel { get; set; }
    public string LogText { get; set; }

    public override CommunicationPacket? Process()
    {
        GwServerPlugin.Logger?.LogWarning("Tried to process out-only LogEntry type packet.");
        return null;
    }
}