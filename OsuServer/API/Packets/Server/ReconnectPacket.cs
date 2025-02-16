using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class ReconnectPacket : ServerPacket
    {
        int Milliseconds;
        public ReconnectPacket(int ms) : base((int) ServerPacketType.Reconnect) 
        {
            Milliseconds = ms;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Milliseconds);
        }
    }
}
