using MySqlConnector;

namespace OsuServer.External.Database
{
    public class DbIntArrayColumn : NullableDbColumn<byte[]>
    {

        public int[]? BlobValue
        {
            get
            {
                if (Value == null) return null;
                return Deserialize(Value);
            }
            set
            {
                base.Value = Serialize(value);
            }
        }

        public DbIntArrayColumn(string name, int[]? value, bool canModify = true) 
            : base(name, Serialize(value), canModify) {}

        public static int[]? Deserialize(MySqlDataReader stream, int ordinal)
        {
            if (stream.IsDBNull(ordinal)) return null;
            byte[] friendsBytes;
            using (var memoryStream = new MemoryStream())
            {
                stream.GetStream(6).CopyTo(memoryStream);
                friendsBytes = memoryStream.ToArray();
            }

            return DbIntArrayColumn.Deserialize(friendsBytes);
        }

        public static int[]? Deserialize(byte[]? value)
        {
            if (value == null) return null;
            int[] ints = new int[value.Length / sizeof(int)];
            Buffer.BlockCopy(value, 0, ints, 0, value.Length);
            return ints;
        }

        public static byte[]? Serialize(int[]? value)
        {
            if (value == null) return null;
            byte[] bytes = new byte[value.Length * sizeof(int)];
            Buffer.BlockCopy(value, 0, bytes, 0, bytes.Length);
            return bytes;
        }

    }
}
