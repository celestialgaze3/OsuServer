using Newtonsoft.Json.Linq;

namespace OsuServer.External.OsuV2Api
{
    public class BeatmapSetExtended : BeatmapSet
    {
        // TODO: implement https://osu.ppy.sh/docs/index.html#beatmapsetextended
        public BeatmapSetExtended(JObject json) : base(json)
        {

        }
    }
}
