using OsuServer.External.OsuV2Api.Responses;

namespace OsuServer.External.OsuV2Api.Requests
{
    public class BeatmapLookupRequest : OsuApiRequest<BeatmapLookupResponse>
    {
        public BeatmapLookupRequest(string? checksum, string? filename, string? id) : base(
            new Uri("https://osu.ppy.sh/api/v2/beatmaps/lookup"),
            HttpMethod.Get,
            EncodeContent(
                new KeyValuePair<string, string?>("checksum", checksum),
                new KeyValuePair<string, string?>("filename", filename),
                new KeyValuePair<string, string?>("id", id)
            )
        )
        { }
    }
}
