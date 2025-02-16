using OsuServer.Objects;
using OsuServer.State;
using OsuServer.Util;
using System.Text;

namespace OsuServer.API.Packets
{
    public static class OsuDataTypes
    {
        public static void WriteOsuString(this BinaryWriter binaryWriter, string content)
        {
            byte[] utf8Content = Encoding.Default.GetBytes(content);

            // Null/empty strings
            if (utf8Content.Length == 0)
            {
                binaryWriter.Write((byte)0);
                return;
            }

            binaryWriter.Write((byte)0x0b); // String identifier
            binaryWriter.WriteULEB128((ulong)utf8Content.LongLength);
            binaryWriter.Write(utf8Content);
        }

        public static string ReadOsuString(this BinaryReader binaryReader)
        {
            byte firstByte = binaryReader.ReadByte();

            // When the first byte is zero, it means it is a null/empty string.
            if (firstByte == 0x00)
            {
                return "";
            }
            else if (firstByte == 0x0b) // String identifier
            {
                int length = (int)binaryReader.ReadULEB128();
                byte[] stringBytes = binaryReader.ReadBytes(length);
                return Encoding.UTF8.GetString(stringBytes);
            }
            else
            {
                throw new InvalidOperationException("There is no osu! string present to be read. " + binaryReader.ToString());
            }

        }

        public static void WriteOsuChannel(this BinaryWriter binaryWriter, Channel channel)
        {
            binaryWriter.WriteOsuString("#" + channel.Name);
            binaryWriter.WriteOsuString(channel.Description);
            binaryWriter.Write((ushort)channel.Members.Count);
        }

        public static OsuMessage ReadOsuMessage(this BinaryReader binaryReader)
        {
            string sender = binaryReader.ReadOsuString();
            string text = binaryReader.ReadOsuString();
            string recipient = binaryReader.ReadOsuString();
            int senderId = binaryReader.ReadInt32();

            return new OsuMessage(sender, text, recipient, senderId);
        }

        public static void WriteOsuMessage(this BinaryWriter binaryWriter, OsuMessage message)
        {
            binaryWriter.WriteOsuString(message.Sender);
            binaryWriter.WriteOsuString(message.Text);
            binaryWriter.WriteOsuString(message.Recipient);
            binaryWriter.Write(message.SenderId);
        }

        public static List<int> ReadIntListShortLength(this BinaryReader binaryReader)
        {
            short length = binaryReader.ReadInt16();
            int bytesRead = 0;
            List<int> result = new List<int>();
            while (bytesRead < (length * 4))
            {
                result.Add(binaryReader.ReadInt32());
                bytesRead += 4;
            }
            return result;
        }

        public static void WriteIntListShortLength(this BinaryWriter binaryWriter, IEnumerable<int> list)
        {
            binaryWriter.Write((short)list.Count());
            foreach (int i in list)
            {
                binaryWriter.Write(i);
            }
        }

        public static List<int> ReadIntListIntLength(this BinaryReader binaryReader)
        {
            int length = binaryReader.ReadInt32();
            int bytesRead = 0;
            List<int> result = new List<int>();
            while (bytesRead < (length * 4))
            {
                result.Add(binaryReader.ReadInt32());
                bytesRead += 4;
            }
            return result;
        }

        public static void WriteIntListIntLength(this BinaryWriter binaryWriter, List<int> list)
        {
            binaryWriter.Write(list.Count);
            foreach (int i in list)
            {
                binaryWriter.Write(i);
            }
        }
        public static MatchData ReadMatchData(this BinaryReader binaryReader)
        {
            short id = binaryReader.ReadInt16();
            bool isInProgress = binaryReader.ReadBoolean();
            byte unused = binaryReader.ReadByte();
            int mods = binaryReader.ReadInt32();
            string name = binaryReader.ReadOsuString();
            string password = binaryReader.ReadOsuString();
            string beatmapName = binaryReader.ReadOsuString();
            int beatmapId = binaryReader.ReadInt32();
            string beatmapMD5 = binaryReader.ReadOsuString();
            byte[] statuses = new byte[16];
            byte[] teams = new byte[16];
            List<int> ids = [];

            for (int i = 0; i < 16; i++)
            {
                statuses[i] = binaryReader.ReadByte();
            }

            for (int i = 0; i < 16; i++)
            {
                teams[i] = binaryReader.ReadByte();
            }

            foreach (var status in statuses)
            {
                // If a player is in this slot
                if ((status & 0b1111100) != 0)
                {
                    ids.Add(binaryReader.ReadInt32());
                }
            }

            int hostId = binaryReader.ReadInt32();
            byte mode = binaryReader.ReadByte();
            byte winCondition = binaryReader.ReadByte();
            byte teamType = binaryReader.ReadByte();
            bool freemod = binaryReader.ReadBoolean();
            int[] slotMods = new int[16];
            if (freemod)
            {
                for (int i = 0; i < 16; i++)
                {
                    slotMods[i] = binaryReader.ReadInt32();
                }
            }

            int seed = binaryReader.ReadInt32();

            return new(id, isInProgress, mods, name, password, beatmapName, beatmapId, beatmapMD5, statuses,
                teams, ids, hostId, mode, winCondition, teamType, freemod, slotMods, seed);
        }

        public static void WriteMatchData(this BinaryWriter binaryWriter, MatchData data, bool sendPassword)
        {
            binaryWriter.Write(data.Id);
            binaryWriter.Write(data.IsInProgress);
            binaryWriter.Write((byte)0); // unused
            binaryWriter.Write(data.Mods);
            binaryWriter.WriteOsuString(data.Name);

            if (data.Password == string.Empty)
            {
                binaryWriter.Write((byte)0); // No password (null byte)
            }
            else if (sendPassword)
            {
                binaryWriter.WriteOsuString(data.Password); // Sending password
            }
            else
            {
                binaryWriter.Write((byte)0x0b); // Password exists
                binaryWriter.Write((byte)0); // But we are not sending it
            }

            binaryWriter.WriteOsuString(data.BeatmapName);
            binaryWriter.Write(data.BeatmapId);
            binaryWriter.WriteOsuString(data.BeatmapMD5);

            foreach (byte status in data.Statuses)
            {
                binaryWriter.Write(status);
            }

            foreach (byte team in data.Teams)
            {
                binaryWriter.Write(team);
            }

            foreach (int id in data.PlayerIds)
            {
                binaryWriter.Write(id);
            }

            binaryWriter.Write(data.HostId);
            binaryWriter.Write(data.Mode);
            binaryWriter.Write(data.WinCondition);
            binaryWriter.Write(data.TeamMode);
            binaryWriter.Write(data.IsFreemod);

            if (data.IsFreemod)
            {
                foreach (int mods in data.SlotMods)
                {
                    binaryWriter.Write(mods);
                }
            }

            binaryWriter.Write(data.Seed);
        }

        public static void WriteLiveScoreData(this BinaryWriter binaryWriter, LiveScoreData data)
        {
            binaryWriter.Write(data.Time);
            binaryWriter.Write(data.Id);
            binaryWriter.Write(data.Perfects);
            binaryWriter.Write(data.Goods);
            binaryWriter.Write(data.Bads);
            binaryWriter.Write(data.Gekis);
            binaryWriter.Write(data.Katus);
            binaryWriter.Write(data.Misses);
            binaryWriter.Write(data.TotalScore);
            binaryWriter.Write(data.MaxCombo);
            binaryWriter.Write(data.CurrentCombo);
            binaryWriter.Write(data.IsPerfect);
            binaryWriter.Write(data.Hp);
            binaryWriter.Write(data.Tag);
            binaryWriter.Write(data.IsScoreV2);

            if (data.IsScoreV2)
            {
                binaryWriter.Write((double)data.ComboPortion);
                binaryWriter.Write((double)data.AccuracyPortion);
            }
        }

        public static LiveScoreData ReadLiveScoreData(this BinaryReader reader)
        {
            int time = reader.ReadInt32();
            byte id = reader.ReadByte();
            ushort perfects = reader.ReadUInt16();
            ushort goods = reader.ReadUInt16();
            ushort bads = reader.ReadUInt16();
            ushort gekis = reader.ReadUInt16();
            ushort katus = reader.ReadUInt16();
            ushort misses = reader.ReadUInt16();
            int totalScore = reader.ReadInt32();
            ushort maxCombo = reader.ReadUInt16();
            ushort currentCombo = reader.ReadUInt16();
            bool isPerfect = reader.ReadBoolean();
            byte hp = reader.ReadByte();
            byte tag = reader.ReadByte();
            bool isScoreV2 = reader.ReadBoolean();
            double? comboPortion = null;
            double? bonusPortion = null;
            if (isScoreV2)
            {
                comboPortion = reader.ReadDouble();
                bonusPortion = reader.ReadDouble();
            }

            return new LiveScoreData(time, id, perfects, goods, bads, gekis, katus, misses, totalScore, maxCombo,
                currentCombo, isPerfect, hp, tag, isScoreV2, comboPortion, bonusPortion);
        }
    }
}
