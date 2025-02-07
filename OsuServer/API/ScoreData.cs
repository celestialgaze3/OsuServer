using OsuServer.External.Database;
using OsuServer.Objects;
using OsuServer.State;

namespace OsuServer.API
{
    public class ScoreData
    {
        public string BeatmapMD5 { get; private set; }
        public string Username { get; private set; }
        public string Checksum { get; private set; }
        public Score Score { get; private set; }
        public string ClientTime { get; private set; }
        public int ClientNum { get; private set; } // Note: info stored here
        public DateTime SubmittedTime { get; private set; }

        private ScoreData(string beatmapMD5, string playerUsername, string checksum, Score score, 
            string clientTime, int clientNum, DateTime submittedTime)
        {
            BeatmapMD5 = beatmapMD5;
            Username = playerUsername;
            Checksum = checksum;
            Score = score;
            ClientTime = clientTime;
            ClientNum = clientNum;
            SubmittedTime = submittedTime;
        }

        /// <summary>
        /// Gets the ScoreData instance from score submission information
        /// </summary>
        /// <param name="database">The active database instance</param>
        /// <param name="bancho">The Bancho server to check</param>
        /// <param name="passwordMD5">The user's password hashed with MD5</param>
        /// <param name="submissionArgs">Score information</param>
        /// <returns>The ScoreData instance, or null if the score is invalid in some way</returns>
        public static async Task<ScoreData?> GetInstance(OsuServerDb database, Bancho bancho, 
            string passwordMD5, string[] submissionArgs)
        {
            string username = submissionArgs[1];
            /* For some reason, if the client has supporter, a space is appended to the username in the score data.
             * Let's remove it. */
            if (username.Last() == ' ') username = username.Substring(0, username.Length - 1);

            /* Get the player instance (since we don't have the osu-token) by the username passwordMD5 combination
            * We aren't just getting by username to prevent spoofed score submissions (while I'm not sure if that's 
            * possible either way). */
            OnlinePlayer? player = bancho.GetPlayer(username, passwordMD5);
            BanchoBeatmap? beatmap = await bancho.GetBeatmap(database, submissionArgs[0]);

            // Has an invalid session...
            if (player == null)
            {
                Console.WriteLine("Score data could not be constructed due to an invalid session");
                return null;
            } 

            // ...or the client submitted a score for a beatmap it thought was ranked
            if (beatmap == null)
            {
                Console.WriteLine("Score data could not be constructed because the beatmap was unsubmitted");
                return null;
            }

            Score score = new(perfects: Int32.Parse(submissionArgs[3]),
                goods: Int32.Parse(submissionArgs[4]),
                bads: Int32.Parse(submissionArgs[5]),
                gekis: Int32.Parse(submissionArgs[6]),
                katus: Int32.Parse(submissionArgs[7]),
                misses: Int32.Parse(submissionArgs[8]),
                totalScore: Int32.Parse(submissionArgs[9]),
                maxCombo: Int32.Parse(submissionArgs[10]),
                perfectCombo: submissionArgs[11] == "True",
                mods: new Mods(Int32.Parse(submissionArgs[13])),
                passed: submissionArgs[14] == "True",
                gameMode: (GameMode)Int32.Parse(submissionArgs[15]),
                player: player,
                beatmap: beatmap,
                timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            );
            return new(submissionArgs[0], username, submissionArgs[2], score,
                submissionArgs[16], Int32.Parse(submissionArgs[17]), DateTime.Now);
        }
    }
}
