namespace GW_server_plugin.Features.IPC.Packets;

/// <summary>
/// Return packet for the response to a command.
/// </summary>
public class ResponsePacket: CommunicationPacket
{
    /// <summary>
    /// The text to send in the response
    /// </summary>
    public string ResponseText { get; set; } = null!;

    /// <inheritdoc />
    public override CommunicationPacket? Process()
    {
        GwServerPlugin.Logger.LogWarning("Tried to process an outgoing only response packet.");
        return null;
    }
}