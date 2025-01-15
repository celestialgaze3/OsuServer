using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class PrivilegesPacket : ServerPacket
    {
        int Privileges;
        public PrivilegesPacket(int privileges, string osuToken, Bancho bancho) : base((int) ServerPacketType.Privileges, osuToken, bancho)
        {
            Privileges = privileges;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Privileges);
        }
    }
}
