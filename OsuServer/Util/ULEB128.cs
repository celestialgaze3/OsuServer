namespace OsuServer.Util
{
    public static class ULEB128
    {
        private static readonly byte MASK_DATA = 0x7f;
        private static readonly byte MASK_CONTINUE = 0x80;

        public static ulong ReadULEB128(this BinaryReader reader)
        {
            ulong value = 0;
            int bitSize = 0;
            byte read;

            do {
                read = reader.ReadByte();

                value |= ((ulong)read & MASK_DATA) << bitSize;
                bitSize += 7;

            } while ((read & MASK_CONTINUE) != 0);
            return value;
        }
        public static byte[] Encode(ulong value)
        {
            using MemoryStream memoryStream = new();

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

        public static void WriteULEB128(this BinaryWriter binaryWriter, ulong value)
        {
            using MemoryStream memoryStream = new();

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
