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
                binaryWriter.Write((byte) 0);
                return;
            }

            binaryWriter.Write((byte) 0x0b); // String identifier
            binaryWriter.WriteULEB128((ulong) utf8Content.LongLength);
            binaryWriter.Write(utf8Content);
        }

        public static string ReadOsuString(this BinaryReader binaryReader)
        {
            byte firstByte = binaryReader.ReadByte();

            // When the first byte is zero, it means it is a null/empty string.
            if (firstByte == 0x00) 
            {
                return "";
            } else if (firstByte == 0x0b) // String identifier
            {
                int length = (int) binaryReader.ReadULEB128();
                byte[] stringBytes = binaryReader.ReadBytes(length);
                return Encoding.UTF8.GetString(stringBytes);
            } else
            {
                throw new InvalidOperationException("There is no osu! string present to be read. " + binaryReader.ToString());
            }

        }

        public static void WriteOsuChannel(this BinaryWriter binaryWriter, Channel channel)
        {
            binaryWriter.WriteOsuString("#" + channel.Name);
            binaryWriter.WriteOsuString(channel.Description);
            binaryWriter.Write((ushort) channel.Members.Count);
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
            binaryWriter.Write((short) list.Count());
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
    }
}
