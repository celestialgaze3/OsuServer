using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class ChannelAutoJoinPacket : ServerPacket
    {
        Channel Channel;
        public ChannelAutoJoinPacket(Channel channel) 
            : base((int) ServerPacketType.ChannelAutoJoin) 
        {
            Channel = channel;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteOsuChannel(Channel);
        }
    }
}
