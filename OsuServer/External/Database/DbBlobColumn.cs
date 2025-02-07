using MySqlConnector;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace OsuServer.External.Database
{
    public abstract class DbBlobColumn<T> : NullableDbColumn<byte[]>
    {
        public abstract T? BlobValue { get; set; }

        public DbBlobColumn(string name, byte[]? value, bool canModify = true) 
            : base(name, value, canModify) {}

    }

    public class DbBlobColumn : DbBlobColumn<byte[]>
    {
        [MemberNotNullWhen(false, nameof(BlobValue))]
        public new bool ValueIsNull => base.ValueIsNull;

        public override byte[]? BlobValue {
            get
            {
                return Value; 
            } 
            set
            {
                Value = value;
            }
        }

        public DbBlobColumn(string name, byte[]? value, bool canModify = true)
            : base(name, value, canModify) { }

        public static byte[]? Deserialize(MySqlDataReader stream, int ordinal)
        {
            if (stream.IsDBNull(ordinal)) return null;
            byte[] bytes;
            using (MemoryStream memoryStream = new())
            {
                stream.GetStream(ordinal).CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            return bytes;
        }
    }
}
