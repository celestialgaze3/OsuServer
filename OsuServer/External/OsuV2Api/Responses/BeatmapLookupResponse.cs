
using Newtonsoft.Json.Linq;

namespace OsuServer.External.OsuV2Api.Responses
{
    public class BeatmapLookupResponse : OsuApiResponse
    {
        public BeatmapExtended BeatmapExtended { get; set; }
        public override async Task<OsuApiResponse> Parse(HttpResponseMessage response)
        {
            string content = await response.Content.ReadAsStringAsync();

            JObject json = JObject.Parse(content);

            if (json.GetValue("id") == null)
                throw await Malformed(response);

            BeatmapExtended = new BeatmapExtended(json);

            return this;
        }
    }
}
