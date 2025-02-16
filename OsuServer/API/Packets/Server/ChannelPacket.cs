using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class ChannelPacket : ServerPacket
    {
        Channel Channel;
        public ChannelPacket(Channel channel) 
            : base((int) ServerPacketType.Channel) 
        {
            Channel = channel;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteOsuChannel(Channel);
        }
    }
}
