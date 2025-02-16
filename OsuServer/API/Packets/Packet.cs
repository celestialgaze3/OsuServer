namespace OsuServer.API.Packets
{
    /// <summary>
    /// Packets are simple: they have an ID and a byte array of their contained information. The data in this array
    /// will depend on what type of packet it is.
    /// </summary>
    public class Packet
    {
        public int Id { get; protected set; }
        public byte[] Data { get; protected set; }

        protected Packet(int id, byte[] data)
        {
            Id = id;
            Data = data;
        }

        public virtual byte[] GetBytes()
        {
            // Packet format: ID (int, 4 bytes), 1 padding byte, data length (int, 4 bytes), then the bytes of data
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.Write((ushort) Id); // ID
                    binaryWriter.Write((byte) 0); // Padding Byte
                    binaryWriter.Write(Data.Length); // Length
                    binaryWriter.Write(Data); // Data
                }

                return stream.ToArray();
            }

        }
    }
}
