using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class ReconnectPacket : ServerPacket
    {
        int Milliseconds;
        public ReconnectPacket(int ms, string osuToken, Bancho bancho) : base((int) ServerPacketType.Reconnect, osuToken, bancho) 
        {
            Milliseconds = ms;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Milliseconds);
        }
    }
}
