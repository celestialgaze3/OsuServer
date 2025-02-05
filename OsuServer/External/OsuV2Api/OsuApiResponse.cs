using OsuServer.External.OsuV2Api.Responses;

namespace OsuServer.External.OsuV2Api
{
    // Covariant return types for static methods on interfaces when?
    public class OsuApiResponse
    {

        public virtual Task<OsuApiResponse?> Parse(HttpResponseMessage response)
        {
            throw new NotImplementedException();
        }

        public async Task<HttpIOException> Malformed(HttpResponseMessage response)
        {
            return new HttpIOException(HttpRequestError.InvalidResponse, $"Received malformed response from the osu! API: \n{await response.Content.ReadAsStringAsync()}");
        }
    }
}
