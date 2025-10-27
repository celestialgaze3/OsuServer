namespace OsuServer.External.Filesystem
{
    public abstract class Repository
    {
        string FolderPath { get; set; }

        public Repository(string folderPath) 
        { 
            FolderPath = folderPath; 
        }

        protected void EnsurePathExists()
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
        }

        protected async Task Write(string filename, byte[] bytes)
        {
            EnsurePathExists();
            await File.WriteAllBytesAsync(Path.Combine(FolderPath, filename), bytes);
        }

        protected async Task Append(string filename, byte[] bytes)
        {
            EnsurePathExists();
            await File.AppendAllBytesAsync(Path.Combine(FolderPath, filename), bytes);
        }

        protected bool Exists(string filename)
        {
            return File.Exists(Path.Combine(FolderPath, filename));
        }

        protected async Task<byte[]> Read(string filename)
        {
            EnsurePathExists();
            return await File.ReadAllBytesAsync(Path.Combine(FolderPath, filename));
        }
    }
}
