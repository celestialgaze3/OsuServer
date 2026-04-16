using Org.BouncyCastle.Asn1.Ocsp;
using OsuServer.External.Filesystem;
using OsuServer.External.OsuV2Api;
using OsuServer.Objects;
using System.Text;

namespace OsuServer.State
{
    public class BanchoBeatmap
    {
        private Bancho _bancho;
        public BeatmapExtended Info { get; private set; }

        private Dictionary<Score, double> _ppCache = [];

        public BanchoBeatmap(Bancho bancho, BeatmapExtended beatmap)
        {
            _bancho = bancho;
            Info = beatmap;
        }

        public async Task<double> CalculatePerformancePoints(Score score)
        {
            double pp;
            if (_ppCache.TryGetValue(score, out pp))
            {
                return pp;
            }

            // Get .osu data, retrieve if needed
            string beatmapData;
            if (BeatmapRepository.Instance.Exists(Info.Id))
            {
                beatmapData = await BeatmapRepository.Instance.Read(Info.Id);
            } 
            else
            {
                StringBuilder requestUriBuilder = new();
                requestUriBuilder.Append($"https://{ServerConfiguration.BeatmapMirrorApiBaseUrl}" +
                    $"{ServerConfiguration.BeatmapMirrorOsuFileDownloadEndpoint}/{Info.Id}");

                using HttpClient client = new();
                HttpRequestMessage apiRequest = new()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new(requestUriBuilder.ToString())
                };
                apiRequest.Headers.Add("User-Agent", "osu!");

                Console.WriteLine($"Making a request to {apiRequest.RequestUri.ToString()}...");
                HttpResponseMessage apiResponse = await client.SendAsync(apiRequest);
                string beatmapString = await apiResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Completed search request.");

                if (!string.IsNullOrEmpty(beatmapString) && apiResponse.IsSuccessStatusCode)
                {
                    await BeatmapRepository.Instance.Write(Info.Id, beatmapString); 
                } else
                {
                    return -1;
                }

                beatmapData = beatmapString;
            }

            // TODO: proper performance points calculation
            // Calculate the number of objects and subtract # of bad judgements
            string[] objects = beatmapData.Split("[HitObjects]")[1].Split("[")[0].Split("\n");
            pp = objects.Length - (score.Goods + score.Bads + score.Misses);

            return pp;
        }

        /// <returns>This beatmap's adjusted ranked status</returns>
        public RankStatus GetRankStatus()
        {
            /* TODO: if i'm interested enough, maybe a custom ranking system one day.
             * but for now, one thing i've always kindof wished for... every map should
             * be ranked (or at least, have a leaderboard). */
            if (!Info.Ranked.ClientSubmitsScores)
                Info.Ranked = RankStatus.Approved;

            return Info.Ranked;
        }

        public bool ShouldAwardStatIncrease()
        {
            return true;
            /*RankStatus status = GetRankStatus();
            if (status == RankStatus.Ranked || status == RankStatus.Approved ||
                status == RankStatus.Loved)
            {
                return true;
            }*/

            // Graveyard, pending, qualified
            //return false;
        }
    }

}
