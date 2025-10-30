using System.Text;

namespace OsuServer.External.Filesystem
{
    public class BeatmapRepository : Repository
    {
        private static BeatmapRepository? _instance = null;
        private static readonly object _padlock = new();

        public static BeatmapRepository Instance
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

        private BeatmapRepository(string folderPath) : base(folderPath) { }

        public static BeatmapRepository Initialize(string folderPath)
        {
            _instance = new BeatmapRepository(folderPath);
            return _instance;
        }

        private string GetFilename(int id)
        {
            return id + ".osu";
        }

        public async Task Write(int id, string beatmapString)
        {
            await Write(GetFilename(id), Encoding.UTF8.GetBytes(beatmapString));
        }

        public bool Exists(int id)
        {
            return Exists(GetFilename(id));
        }
        
        public async Task<string> Read(int id)
        {
            return Encoding.UTF8.GetString(await Read(GetFilename(id)));
        }
    }
}
