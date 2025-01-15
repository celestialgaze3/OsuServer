using System.Runtime.InteropServices;

namespace OsuServer.Util
{
    // Lazily ported to C# from whatever my old java project took from
    public static class ULEB128
    {
        private static byte MASK_DATA = 0x7f;
        private static byte MASK_CONTINUE = 0x80;

        public static ulong ReadULEB128(this BinaryReader reader)
        {
            ulong value = 0;
            int bitSize = 0;
            int read;

            do {
                if ((read = reader.Read()) == -1)
                {
                    throw new IOException("Unexpected EOF");
                }

                value += ((ulong)read & MASK_DATA) << bitSize;
                bitSize += 7;

            } while ((read & MASK_CONTINUE) != 0);
            return value;
        }
        public static byte[] Encode(ulong value)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                do
                {
                    byte b = (byte)(value & MASK_DATA);
                    value >>= 7;
                    if (value != 0)
                    {
                        b |= MASK_CONTINUE;
                    }
                    memoryStream.WriteByte((byte)b);
                } while (value != 0);

                return memoryStream.ToArray();
            }
        }

        public static void WriteULEB128(this BinaryWriter binaryWriter, ulong value)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                do
                {
                    byte b = (byte)(value & MASK_DATA);
                    value >>= 7;
                    if (value != 0)
                    {
                        b |= MASK_CONTINUE;
                    }
                    memoryStream.WriteByte((byte)b);
                } while (value != 0);

                binaryWriter.Write(memoryStream.ToArray());
            }
        }
    }
}
