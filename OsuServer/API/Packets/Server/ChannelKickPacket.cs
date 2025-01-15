using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class ChannelKickPacket : ServerPacket
    {
        string Name { get; set; }
        public ChannelKickPacket(string name, string osuToken, Bancho bancho) : base((int) ServerPacketType.ChannelKick, osuToken, bancho) 
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
