using UnityEngine;
using UnityEngine.Networking;

namespace VoiceChat.Networking
{
    //TODO: Fix calls to this class to use netId instead of proxy garbo
    public class VoiceChatPacketMessage : MessageBase
    {
        public short netId;
        public VoiceChatPacket packet;

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(netId);
            writer.Write(packet.PacketId);
            writer.Write((short)packet.Compression);
            writer.Write(packet.Length);
            writer.WriteBytesFull(packet.Data);
        }

        public override void Deserialize(NetworkReader reader)
        {
            netId = reader.ReadInt16();
            packet.PacketId = reader.ReadUInt64();
            packet.Compression = (VoiceChatCompression)reader.ReadInt16();
            packet.Length = reader.ReadInt32();
            packet.Data = reader.ReadBytesAndSize();
        }
    }
}
