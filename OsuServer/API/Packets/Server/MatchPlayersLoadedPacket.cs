using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class MatchPlayersLoadedPacket : ServerPacket
    {
        public MatchPlayersLoadedPacket(Bancho bancho) 
            : base((int) ServerPacketType.MatchPlayersLoaded) { }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            // No data needs to be written
        }
    }
}
