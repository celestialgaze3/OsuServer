using Newtonsoft.Json.Linq;

namespace OsuServer.External.OsuV2Api
{
    public class Beatmap
    {
        public int BeatmapSetId { get; set; }
        public float DifficultyRating { get; set; }
        public int Id { get; set; }
        public Ruleset Mode { get; set; }
        public RankStatus Status { get; set; } 
        public int TotalLength { get; set; }
        public int UserId { get; set; }
        public string Version { get; set; }
        public BeatmapSet? BeatmapSet { get; set; }
        public string? Checksum { get; set; }
        public int[]? FailTimes { get; set; }
        public int? MaxCombo { get; set; }

        public Beatmap(JToken json)
        {
            // TODO: make some extension method thingy that would take care of invalid data sent from api and remove these warnings
            BeatmapSetId = (int)json["beatmapset_id"];
            DifficultyRating = (float) json["difficulty_rating"];
            Id = (int) json["id"];
            Mode = Ruleset.FromString((string) json["mode"]);
            Status = RankStatus.FromString((string) json["status"]);
            TotalLength = (int) json["total_length"];
            UserId = (int) json["user_id"];
            Version = (string) json["version"];

            // Dont care
            BeatmapSet = new BeatmapSet(json["beatmapset"]);
            Checksum = (string?) json["checksum"];
            //FailTimes = failTimes;
            MaxCombo = (int?) json["max_combo"];
        }
    }
}
