using OsuServer.State;

namespace OsuServer.API.Packets.Server
{
    public class UserStatsPacket : ServerPacket
    {
        Player Player;
        public UserStatsPacket(Player player, string osuToken, Bancho bancho) : base((int) ServerPacketType.UserStats, osuToken, bancho) 
        {
            Player = player;
        }

        protected override void WriteData(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Player.Id); // player id
            binaryWriter.Write((byte) Player.Status.Action); // action id
            binaryWriter.WriteOsuString(Player.Status.InfoText); // info text
            binaryWriter.WriteOsuString(Player.Status.MapMD5); // map md5
            binaryWriter.Write(Player.Status.Mods.IntValue); // mods
            binaryWriter.Write((byte) Player.Status.GameMode); // mode id
            binaryWriter.Write(Player.Status.MapID); // mnap id
            binaryWriter.Write(Player.Stats.Values.RankedScore); // ranked score
            binaryWriter.Write(Player.Stats.Values.Accuracy); // acc
            binaryWriter.Write(Player.Stats.Values.Playcount); // playcount
            binaryWriter.Write(Player.Stats.Values.TotalScore); // total scotre
            binaryWriter.Write(Player.Stats.Values.Rank); // rank
            binaryWriter.Write((short) Player.Stats.Values.PP); //pp
        }
    }
}
