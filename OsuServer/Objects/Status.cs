namespace OsuServer.Objects
{
    public class Status
    {
        public Action Action { get; set; } = Action.Idle;
        public string InfoText { get; set; } = "";
        public string MapMD5 { get; set; } = "";
        public Mods Mods { get; set; } = new Mods(Mod.None);
        public GameMode GameMode { get; set; } = GameMode.Standard;
        public int MapID { get; set; } = 0;

        public Status() { }
    }
}
