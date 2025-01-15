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
        public int ModeInt { get; set; }
        public int Passcount { get; set; }
        public int Playcount { get; set; }
        public RankStatus RankStatus { get; set; }
        public string Url { get; set; }

        public BeatmapExtended(JToken json)
            : base(json)
        {
            Accuracy = (float) json["accuracy"];
            ApproachRate = (float)json["ar"];
            BPM = (float?)json["bpm"];
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
            ModeInt = (int)json["mode_int"];
            Passcount = (int)json["passcount"];
            Playcount = (int)json["playcount"];
            RankStatus = RankStatus.FromInt((int)json["ranked"]);
            Url = (string)json["url"];

            // TODO: make some extension method thingy that would take care of invalid data sent from api and remove these warnings
        }
    }
}
