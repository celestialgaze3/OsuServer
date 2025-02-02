using Microsoft.AspNetCore.Mvc.Diagnostics;
using MySqlConnector;
using OsuServer.Objects;
using System.Xml.Linq;

namespace OsuServer.External.Database.Rows
{
    public class DbScore : DbRow
    {
        public DbColumn<uint> Id { get; }
        public DbColumn<uint> AccountId { get; }
        public DbColumn<uint> BeatmapId { get; }
        public DbColumn<int> Perfects { get; }
        public DbColumn<int> Goods { get; }
        public DbColumn<int> Bads { get; }
        public DbColumn<int> Gekis { get; }
        public DbColumn<int> Katus { get; }
        public DbColumn<int> Misses { get; }
        public DbColumn<int> TotalScore { get; }
        public DbColumn<int> MaxCombo { get; }
        public DbColumn<bool> IsPerfectCombo { get; }
        public DbColumn<int> Mods { get; }
        public DbColumn<bool> IsPass { get; }
        public DbColumn<byte> GameMode { get; }
        public DbColumn<double> PP { get; }

        public DbScore(uint id, uint accountId, uint beatmapId, int perfects, int goods, int bads, int gekis, int katus, int misses,
            int totalScore, int maxCombo, bool isPerfectCombo, int mods, bool isPass, byte gameMode, double pp)
        {
            Id = new("id", id, false);
            AccountId = new("account_id", accountId);
            BeatmapId = new("beatmap_id", beatmapId);
            Perfects = new("perfects", perfects);
            Goods = new("goods", goods);
            Bads = new("bads", bads);
            Gekis = new("gekis", gekis);
            Katus = new("katus", katus);
            Misses = new("misses", misses);
            TotalScore = new("total_score", totalScore);
            MaxCombo = new("max_combo", maxCombo);
            IsPerfectCombo = new("is_perfect_combo", isPerfectCombo);
            Mods = new("mods", mods);
            IsPass = new("is_pass", isPass);
            GameMode = new("gamemode", gameMode);
            PP = new("pp", pp);
        }

        public DbScore(uint id, Score score)
        {
            Id = new("id", id);
            AccountId = new("account_id", (uint) score.Player.Id);
            BeatmapId = new("beatmap_id", (uint) score.Beatmap.Info.Id);
            Perfects = new("perfects", score.Perfects);
            Goods = new("goods", score.Goods);
            Bads = new("bads", score.Bads);
            Gekis = new("gekis", score.Gekis);
            Katus = new("katus", score.Katus);
            Misses = new("misses", score.Misses);
            TotalScore = new("total_score", score.TotalScore);
            MaxCombo = new("max_combo", score.MaxCombo);
            IsPerfectCombo = new("is_perfect_combo", score.PerfectCombo);
            Mods = new("mods", score.Mods.IntValue);
            IsPass = new("is_pass", score.Passed);
            GameMode = new("gamemode", (byte) score.GameMode);
            PP = new("pp", score.Beatmap.CalculatePerformancePoints(score));
        }

        public override DbColumn[] GetColumns()
        {
            return [Id, AccountId, BeatmapId, Perfects, Goods, Bads, Gekis, Katus, Misses, 
                TotalScore, MaxCombo, IsPerfectCombo, Mods, IsPass, GameMode, PP];
        }

        public override DbColumn[] GetIdentifyingColumns()
        {
            return [Id];
        }
    }
}
