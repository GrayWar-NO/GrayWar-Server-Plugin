using System;
using GW_server_plugin.Features.IPC.Packets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GW_server_plugin.Features.IPC;

public class PacketTypeConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(CommunicationPacket).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader,
        Type objectType,
        object? existingValue,
        JsonSerializer serializer)
    {
        var jo = JObject.Load(reader);
        var type = jo["type"]!.ToObject<PacketType>();
        CommunicationPacket? packet;

        switch (type)
        {
            case PacketType.Ping:
                packet = new PingPacket();
                break;
            case PacketType.Command:
                packet = new CommandPacket();
                break;
            case PacketType.Response:
                packet = new ResponsePacket();
                break;
            case PacketType.LogEntry:
                packet = new LogEntryPacket();
                break;
            default:
                GwServerPlugin.Logger?.LogError($"Unknown packet type {type}");
                throw new ArgumentOutOfRangeException();
        }
        
        serializer.Populate(jo.CreateReader(), packet);
        return packet;
    }
    
    public override void WriteJson(JsonWriter writer,
        object? value,
        JsonSerializer serializer)
    {
        var packet = (CommunicationPacket)value!;
        writer.WriteStartObject();
        
        writer.WritePropertyName("Type");
        serializer.Serialize(writer, packet.Type.ToString().ToLower());
        
        switch (packet)
        {
            case PingPacket ping:
                writer.WritePropertyName("Data");
                serializer.Serialize(writer, ping.Data);
                break;
            case CommandPacket cmd:
                writer.WritePropertyName("CommandName");
                serializer.Serialize(writer, cmd.CommandName);
                writer.WritePropertyName("Parameters");
                serializer.Serialize(writer, cmd.Parameters);
                break;
            case ResponsePacket resp:
                writer.WritePropertyName("ResponseText");
                serializer.Serialize(writer, resp.ResponseText);
                break;
            case LogEntryPacket log:
                writer.WritePropertyName("Channel");
                serializer.Serialize(writer, log.Channel.ToString().ToLower());
                writer.WritePropertyName("LogText");
                serializer.Serialize(writer, log.LogText);
                break;
        }

        writer.WriteEndObject();
    }    
}