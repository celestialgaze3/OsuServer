using Newtonsoft.Json.Linq;

namespace OsuServer.External.OsuV2Api
{
    public class BeatmapExtended : Beatmap
    {
        public float Accuracy { get; set; }
        public float ApproachRate { get; set; }
        public float? BPM { get; set; }
        public bool IsConvert { get; set; }
        public int CountCircles { get; set; }
        public int CountSliders { get; set; }
        public int CountSpinners { get; set; }
        public float CircleSize { get; set; }
        public DateTime? DeletedAt { get; set; }
        public float HPDrain { get; set; }
        public int HitLength { get; set; }
        public bool IsScoreable { get; set; }
        public DateTime LastUpdated {  get; set; }
        public Ruleset ModeInt { get; set; }
        public int Passcount { get; set; }
        public int Playcount { get; set; }
        public RankStatus Ranked { get; set; }
        public string Url { get; set; }

        public BeatmapExtended(JObject json)
            : base(json)
        {
            Accuracy = (float) json["accuracy"];
            ApproachRate = (float)json["ar"];
            BPM = (float?)json["bpm"];
            Console.WriteLine(BPM);
            IsConvert = (bool)json["convert"];
            CountCircles = (int)json["count_circles"];
            CountSliders = (int)json["count_sliders"];
            CountSpinners = (int)json["count_spinners"];
            CircleSize = (float)json["cs"];
            string? timeDeleted = (string?) json["deleted_at"];
            DeletedAt = timeDeleted != null ? DateTime.Parse(timeDeleted, null, System.Globalization.DateTimeStyles.RoundtripKind) : null;
            HPDrain = (float)json["drain"];
            HitLength = (int)json["hit_length"];
            IsScoreable = (bool)json["is_scoreable"];
            LastUpdated = DateTime.Parse((string) json["last_updated"], null, System.Globalization.DateTimeStyles.RoundtripKind);
            ModeInt = Ruleset.FromInt((int)json["mode_int"]);
            Passcount = (int)json["passcount"];
            Playcount = (int)json["playcount"];
            Ranked = RankStatus.FromInt((int)json["ranked"]);
            Url = (string)json["url"];

            // TODO: make some extension method thingy that would take care of invalid data sent from api and remove these warnings
        }

        public BeatmapExtended(int id, int beatmapSetId, float difficultyRating, Ruleset mode, RankStatus status,
            int totalLength, int userId, string version, BeatmapSet? beatmapSet, string? checksum, int[]? failTimes,
            int[]? exitTimes, int? maxCombo, float accuracy, float approachRate, float? bpm, bool isConvert, 
            int countCircles, int countSliders, int countSpinners, float circleSize, DateTime? deletedAt, float hPDrain, 
            int hitLength, bool isScoreable, DateTime lastUpdated, int passcount, int playcount, string url) 
                : base(id, beatmapSetId, difficultyRating, mode, status, totalLength, userId, version, beatmapSet,
                checksum, failTimes, exitTimes, maxCombo)
        {
            Accuracy = accuracy;
            ApproachRate = approachRate;
            BPM = bpm;
            IsConvert = isConvert;
            CountCircles = countCircles;
            CountSliders = countSliders;
            CountSpinners = countSpinners;
            CircleSize = circleSize;
            DeletedAt = deletedAt;
            HPDrain = hPDrain;
            HitLength = hitLength;
            IsScoreable = isScoreable;
            LastUpdated = lastUpdated;
            ModeInt = mode;
            Passcount = passcount;
            Playcount = playcount;
            Ranked = status;
            Url = url;
        }
    }
}
