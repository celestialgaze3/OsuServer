using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class ChannelJoinSuccessPacket : ServerPacket
    {
        string Name { get; set; }
        public ChannelJoinSuccessPacket(string name) 
            : base((int) ServerPacketType.ChannelJoinSuccess) 
        {
            Name = name;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteOsuString("#" + Name);
            Console.WriteLine("Channel join success " + Name);
        }
    }
}
