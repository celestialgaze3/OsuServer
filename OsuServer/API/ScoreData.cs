using OsuServer.Objects;
using OsuServer.State;

namespace OsuServer.API
{
    public class ScoreData
    {
        public string BeatmapMD5 { get; private set; }
        public string PlayerUsername { get; private set; }
        public string Checksum { get; private set; }
        public Score Score { get; private set; }
        public string PlayTime { get; private set; }
        public int ClientNum { get; private set; } // Note: info stored here
        public DateTime SubmittedTime { get; private set; }

        public ScoreData(string[] submissionArgs)
        {
            BeatmapMD5 = submissionArgs[0];
            PlayerUsername = submissionArgs[1];
            Checksum = submissionArgs[2]; // "chickenmcnuggets{0}o15{1}{2}smustard{3}{4}uu{5}{6}{7}{8}{9}{10}{11}Q{12}{13}{15}{14:%y%m%d%H%M%S}{16}{17}"

            Score = new(perfects: Int32.Parse(submissionArgs[3]), 
                goods: Int32.Parse(submissionArgs[4]), 
                bads: Int32.Parse(submissionArgs[5]),
                gekis: Int32.Parse(submissionArgs[6]),
                katus: Int32.Parse(submissionArgs[7]), 
                misses: Int32.Parse(submissionArgs[8]), 
                totalScore: Int32.Parse(submissionArgs[9]), 
                maxCombo: Int32.Parse(submissionArgs[10]),
                perfectCombo: submissionArgs[11] == "True", 
                grade: Enum.Parse<Grade>(submissionArgs[12]), 
                mods: new Mods(Int32.Parse(submissionArgs[13])), 
                passed: submissionArgs[14] == "True",
                gameMode: (GameMode) Int32.Parse(submissionArgs[15])
            );

            PlayTime = submissionArgs[16]; // TODO: parse DateTime
            ClientNum = Int32.Parse(submissionArgs[17]);
            SubmittedTime = DateTime.Now;
        }
    }
}
