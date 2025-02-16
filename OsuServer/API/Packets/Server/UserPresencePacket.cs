using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class UserPresencePacket : ServerPacket
    {
        OnlinePlayer Player;
        public UserPresencePacket(OnlinePlayer player) 
            : base((int) ServerPacketType.UserPresence) 
        {
            Player = player;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Player.Id); // player id
            binaryWriter.WriteOsuString(Player.Username); // username
            binaryWriter.Write((byte)(Player.Presence.UtcOffset + 24)); // utc offset
            binaryWriter.Write((byte)Player.Presence.Geolocation.CountryCode); // country code
            binaryWriter.Write(Player.Privileges.IntValue); // privileges
            binaryWriter.Write(Player.Presence.Geolocation.Longitude); // longitude
            binaryWriter.Write(Player.Presence.Geolocation.Latitude); // latitude
            binaryWriter.Write(Player.Stats[Player.Status.GameMode].Values.Rank); // rank
        }
    }
}
