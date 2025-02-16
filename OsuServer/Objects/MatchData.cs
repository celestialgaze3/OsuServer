namespace OsuServer.Objects
{
    public class MatchData
    {
        public short Id { get; set; }
        public bool IsInProgress { get; set; }
        public int Mods { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string BeatmapName { get; set; }
        public int BeatmapId { get; set; }
        public string BeatmapMD5 { get; set; }
        public byte[] Statuses { get; set; }
        public byte[] Teams { get; set; }
        public List<int> PlayerIds { get; set; }
        public int HostId { get; set; }
        public byte Mode { get; set; }
        public byte WinCondition { get; set; }
        public byte TeamMode { get; set; }
        public bool IsFreemod { get; set; }
        public int[] SlotMods { get; set; }
        public int Seed { get; set; }

        public MatchData(short id, bool isInProgress, int mods, string name, string password, string beatmapName, 
            int beatmapId, string beatmapMD5, byte[] statuses, byte[] teams, List<int> slotIds, 
            int hostId, byte mode, byte winCondition, byte teamMode, bool freemod, int[] slotMods, int seed)
        {
            Id = id;
            IsInProgress = isInProgress;
            Mods = mods;
            Name = name;
            Password = password;
            BeatmapName = beatmapName;
            BeatmapId = beatmapId;
            BeatmapMD5 = beatmapMD5;
            Statuses = statuses;
            Teams = teams;
            PlayerIds = slotIds;
            HostId = hostId;
            Mode = mode;
            WinCondition = winCondition;
            TeamMode = teamMode;
            IsFreemod = freemod;
            SlotMods = slotMods;
            Seed = seed;
            //Log();
        }
        /*
        public void Log()
        {
            Console.WriteLine($"Id: {Id}");
            Console.WriteLine($"IsInProgress: {IsInProgress}");
            Console.WriteLine($"Mods: {Mods}");
            Console.WriteLine($"Name: {Name}");
            Console.WriteLine($"Password: {Password}");
            Console.WriteLine($"BeatmapName: {BeatmapName}");
            Console.WriteLine($"BeatmapId: {BeatmapId}");
            Console.WriteLine($"BeatmapMD5: {BeatmapMD5}");
            Console.WriteLine($"Statuses: {Statuses.Length}");
            foreach (var item in Statuses)
            {
                Console.Write(item + ",");
            }
            Console.WriteLine();
            Console.WriteLine($"Teams: {Teams.Length}");
            foreach (var item in Teams)
            {
                Console.Write(item + ",");
            }
            Console.WriteLine();
            Console.WriteLine($"PlayerIds: {PlayerIds.Count}");
            foreach (var item in PlayerIds)
            {
                Console.Write(item + ",");
            }
            Console.WriteLine();
            Console.WriteLine($"HostId: {HostId}");
            Console.WriteLine($"Mode: {Mode}");
            Console.WriteLine($"WinCondition: {WinCondition}");
            Console.WriteLine($"TeamMode: {TeamMode}");
            Console.WriteLine($"IsFreemod: {IsFreemod}");
            Console.WriteLine($"SlotMods: {SlotMods.Length}");
            foreach (var item in SlotMods)
            {
                Console.Write(item + ",");
            }
            Console.WriteLine();
            Console.WriteLine($"Seed: {Seed}");
        }*/
    }
}
