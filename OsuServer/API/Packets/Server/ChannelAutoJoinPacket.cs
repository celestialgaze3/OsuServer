using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class ChannelAutoJoinPacket : ServerPacket
    {
        Channel Channel;
        public ChannelAutoJoinPacket(Channel channel, string osuToken, Bancho bancho) : base((int) ServerPacketType.ChannelAutoJoin, osuToken, bancho) 
        {
            Channel = channel;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteOsuChannel(Channel);
        }
    }
}
