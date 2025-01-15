using Newtonsoft.Json.Linq;

namespace OsuServer.External.OsuV2Api
{
    public class BeatmapSetExtended : BeatmapExtended
    {
        // TODO: implement https://osu.ppy.sh/docs/index.html#beatmapsetextended
        public BeatmapSetExtended(JToken json) : base(json)
        {

        }
    }
}
