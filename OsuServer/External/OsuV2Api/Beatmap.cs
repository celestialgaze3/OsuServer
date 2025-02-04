using Newtonsoft.Json;
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
        public int[]? ExitTimes { get; set; }
        public int? MaxCombo { get; set; }

        public string? FullName
        {
            get
            {
                if (BeatmapSet == null) return null;
                return $"{BeatmapSet.Artist} - {BeatmapSet.Title} [{Version}]";
            }
        }

        public Beatmap(JObject json)
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
            BeatmapSet = new BeatmapSet(json["beatmapset"]);
            Checksum = (string?) json["checksum"];
            if (json.ContainsKey("failtimes"))
            {
                FailTimes = JsonConvert.DeserializeObject<int[]>(json["failtimes"]["fail"].ToString());
                ExitTimes = JsonConvert.DeserializeObject<int[]>(json["failtimes"]["exit"].ToString());
            }
            MaxCombo = (int?) json["max_combo"];
        }

        public Beatmap(int id, int beatmapSetId, float difficultyRating, Ruleset mode, RankStatus status, 
            int totalLength, int userId, string version, BeatmapSet? beatmapSet, string? checksum, int[]? failTimes,
            int[]? exitTimes, int? maxCombo)
        {
            BeatmapSetId = beatmapSetId;
            DifficultyRating = difficultyRating;
            Id = id;
            Mode = mode;
            Status = status;
            TotalLength = totalLength;
            UserId = userId;
            Version = version;
            BeatmapSet = beatmapSet;
            Checksum = checksum;
            FailTimes = failTimes;
            ExitTimes = exitTimes;
            MaxCombo = maxCombo;
        }
    }
}
