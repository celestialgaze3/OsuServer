using OsuServer.External.OsuV2Api;

namespace OsuServer.External.Database.Rows
{
    public class DbBeatmap : DbRow
    {
        public DbColumn<int> Id { get; set; }
        public DbColumn<int> BeatmapSetId { get; set; }
        public DbColumn<float> DifficultyRating { get; set; }
        public DbColumn<int> Mode { get; set; }
        public DbColumn<int> RankedStatus { get; set; }
        public DbColumn<int> TotalLength { get; set; }
        public DbColumn<int> UserId { get; set; }
        public DbColumn<string> Version { get; set; }
        public NullableDbColumn<string?> Checksum { get; set; }
        public NullableDbColumn<int?> MaxCombo { get; set; }
        public DbColumn<float> Accuracy { get; set; }
        public DbColumn<float> ApproachRate { get; set; }
        public NullableDbColumn<float?> BPM { get; set; }
        public DbColumn<bool> IsConvert { get; set; }
        public DbColumn<int> CountCircles { get; set; }
        public DbColumn<int> CountSliders { get; set; }
        public DbColumn<int> CountSpinners { get; set; }
        public DbColumn<float> CircleSize { get; set; }
        public NullableDbColumn<DateTime?> DeletedAt { get; set; }
        public DbColumn<float> HPDrain { get; set; }
        public DbColumn<int> HitLength { get; set; }
        public DbColumn<bool> IsScoreable { get; set; }
        public DbColumn<DateTime> LastUpdated { get; set; }
        public DbColumn<int> Passcount { get; set; }
        public DbColumn<int> Playcount { get; set; }

        public DbBeatmap(BeatmapExtended b) 
        {
            Id = new("id", b.Id);
            BeatmapSetId = new("beatmapset_id", b.BeatmapSetId);
            DifficultyRating = new("difficulty_rating", b.DifficultyRating);
            Mode = new("mode", b.ModeInt);
            RankedStatus = new("ranked_status", b.Ranked);
            TotalLength = new("total_length", b.TotalLength);
            UserId = new("user_id", b.UserId);
            Version = new("version", b.Version);
            Checksum = new("checksum", b.Checksum);
            MaxCombo = new("max_combo", b.MaxCombo);
            Accuracy = new("overall_difficulty", b.Accuracy);
            ApproachRate = new("approach_rate", b.ApproachRate);
            BPM = new("bpm", b.BPM);
            IsConvert = new("is_convert", b.IsConvert);
            CountCircles = new("circle_count", b.CountCircles);
            CountSliders = new("slider_count", b.CountSliders);
            CountSpinners = new("spinner_count", b.CountSpinners);
            CircleSize = new("circle_size", b.CircleSize);
            DeletedAt = new("deleted_at", b.DeletedAt);
            HPDrain = new("hp_drain", b.HPDrain);
            HitLength = new("hit_length", b.HitLength);
            IsScoreable = new("is_scoreable", b.IsScoreable);
            LastUpdated = new("last_updated", b.LastUpdated);
            Passcount = new("passcount", b.Passcount);
            Playcount = new("playcount", b.Playcount);
        }
        
        public BeatmapExtended BeatmapExtended => new(Id.Value, BeatmapSetId.Value, DifficultyRating.Value, 
            Ruleset.FromInt(Mode.Value), RankStatus.FromInt(RankedStatus.Value), TotalLength.Value, UserId.Value, 
            Version.Value, null, Checksum.Value, null, null, MaxCombo.Value, Accuracy.Value, ApproachRate.Value, 
            BPM.Value, IsConvert.Value, CountCircles.Value, CountSliders.Value, CountSpinners.Value, CircleSize.Value, 
            DeletedAt.Value, HPDrain.Value, HitLength.Value, IsScoreable.Value, LastUpdated.Value, Passcount.Value, 
            Playcount.Value, $"https://osu.ppy.sh/b/{Id.Value}");

        public override DbColumn[] GetColumns()
        {
            return [Id, BeatmapSetId, DifficultyRating, Mode, RankedStatus, TotalLength, UserId, Version,
                Checksum, MaxCombo, Accuracy, ApproachRate, BPM, IsConvert, CountCircles, CountSliders,
                CountSpinners, CircleSize, DeletedAt, HPDrain, HitLength, IsScoreable, LastUpdated, Passcount, 
                Playcount];
        }

        public override DbColumn[] GetIdentifyingColumns()
        {
            return [Id];
        }
    }
}
