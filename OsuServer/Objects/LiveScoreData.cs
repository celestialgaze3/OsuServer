using System.Diagnostics.CodeAnalysis;

namespace OsuServer.Objects
{
    public class LiveScoreData
    {
        public int Time { get; set; }
        public byte Id { get; set; }
        public ushort Perfects { get; set; }
        public ushort Goods { get; set; }
        public ushort Bads { get; set; }
        public ushort Gekis { get; set; }
        public ushort Katus { get; set; }
        public ushort Misses { get; set; }
        public int TotalScore { get; set; }
        public ushort MaxCombo { get; set; }
        public ushort CurrentCombo { get; set; }
        public bool IsPerfect { get; set; }
        public byte Hp { get; set; }
        public byte Tag { get; set; }

        [MemberNotNullWhen(true, nameof(ComboPortion), nameof(AccuracyPortion))]
        public bool IsScoreV2 { get; set; }
        public double? ComboPortion { get; set; }
        public double? AccuracyPortion { get; set; }

        public LiveScoreData(int time, byte id, ushort perfects, ushort goods, ushort bads, ushort gekis, ushort katus, 
            ushort misses, int totalScore, ushort maxCombo, ushort currentCombo, bool isPerfect, byte currentHp, byte tag, 
            bool isScoreV2, double? comboPortion, double? accuracyPortion)
        {
            Time = time;
            Id = id;
            Perfects = perfects;
            Goods = goods;
            Bads = bads;
            Gekis = gekis;
            Katus = katus;
            Misses = misses;
            TotalScore = totalScore;
            MaxCombo = maxCombo;
            CurrentCombo = currentCombo;
            IsPerfect = isPerfect;
            Hp = currentHp;
            Tag = tag;
            IsScoreV2 = isScoreV2;
            ComboPortion = comboPortion;
            AccuracyPortion = accuracyPortion;
        }
    }
}
