namespace OsuServer.External.Filesystem
{
    public class ReplayRepository : Repository
    {
        private static ReplayRepository? _instance = null;
        private static readonly object _padlock = new();

        public static ReplayRepository Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                    {
                        throw new InvalidOperationException("The repository has not been initialized.");
                    }
                    return _instance;
                }
            }
        }

        private ReplayRepository(string folderPath) : base(folderPath) { }

        public static ReplayRepository Initialize(string folderPath)
        {
            _instance = new ReplayRepository(folderPath);
            return _instance;
        }

        private string GetFilename(int id)
        {
            return id + ".osr";
        }

        public async Task Write(int id, byte[] bytes)
        {
            await Write(GetFilename(id), bytes);
        }

        public bool Exists(int id)
        {
            return Exists(GetFilename(id));
        }
        
        public async Task<byte[]> Read(int id)
        {
            return await Read(GetFilename(id));
        }
    }
}
