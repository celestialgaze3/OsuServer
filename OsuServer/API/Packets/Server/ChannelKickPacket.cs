using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class ChannelKickPacket : ServerPacket
    {
        string Name { get; set; }
        public ChannelKickPacket(string name) 
            : base((int) ServerPacketType.ChannelKick) 
        {
            Name = name;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteOsuString("#" + Name);
            Console.WriteLine("Kicked from channel " + Name);
        }
    }
}
