using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class ChannelPacket : ServerPacket
    {
        Channel Channel;
        public ChannelPacket(Channel channel, string osuToken, Bancho bancho) : base((int) ServerPacketType.Channel, osuToken, bancho) 
        {
            Channel = channel;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteOsuChannel(Channel);
        }
    }
}
