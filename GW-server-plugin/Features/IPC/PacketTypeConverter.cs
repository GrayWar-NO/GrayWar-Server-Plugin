using System;
using GW_server_plugin.Enums;
using GW_server_plugin.Features.IPC.Packets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GW_server_plugin.Features.IPC;

/// <summary>
/// Json converter for handling packets.
/// </summary>
public class PacketTypeConverter : JsonConverter
{
    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return typeof(CommunicationPacket).IsAssignableFrom(objectType);
    }

    /// <inheritdoc />
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
                GwServerPlugin.Logger.LogError($"Unknown packet type {type}");
                throw new ArgumentOutOfRangeException();
        }
        
        serializer.Populate(jo.CreateReader(), packet);
        return packet;
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer,
        object? value,
        JsonSerializer serializer)
    {
        var packet = (CommunicationPacket)value!;
        writer.WriteStartObject();
        
        writer.WritePropertyName("type");
        serializer.Serialize(writer, packet.Type.ToString().ToLower());
        
        switch (packet)
        {
            case PingPacket ping:
                writer.WritePropertyName("data");
                serializer.Serialize(writer, ping.Data);
                break;
            case CommandPacket cmd:
                writer.WritePropertyName("commandName");
                serializer.Serialize(writer, cmd.CommandName);
                writer.WritePropertyName("arguments");
                serializer.Serialize(writer, cmd.Arguments);
                writer.WritePropertyName("result");
                serializer.Serialize(writer, cmd.Result);
                break;
            case ResponsePacket resp:
                writer.WritePropertyName("responseText");
                serializer.Serialize(writer, resp.ResponseText);
                break;
            case ChatLogPacket log:
                writer.WritePropertyName("chatName");
                serializer.Serialize(writer, log.ChatName);
                writer.WritePropertyName("channel");
                serializer.Serialize(writer, log.Channel.ToString().ToLower());
                writer.WritePropertyName("logText");
                serializer.Serialize(writer, log.LogText);
                writer.WritePropertyName("steamID");
                serializer.Serialize(writer, log.SteamID);
                break;
            case LogEntryPacket log:
                writer.WritePropertyName("channel");
                serializer.Serialize(writer, log.Channel.ToString().ToLower());
                writer.WritePropertyName("logText");
                serializer.Serialize(writer, log.LogText);
                break;
        }

        writer.WriteEndObject();
    }    
}