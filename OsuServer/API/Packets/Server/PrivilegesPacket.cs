using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class PrivilegesPacket : ServerPacket
    {
        int Privileges;
        public PrivilegesPacket(int privileges) 
            : base((int) ServerPacketType.Privileges)
        {
            Privileges = privileges;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Privileges);
        }
    }
}
