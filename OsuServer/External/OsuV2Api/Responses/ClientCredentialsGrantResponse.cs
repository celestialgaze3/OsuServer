
using Newtonsoft.Json.Linq;

namespace OsuServer.External.OsuV2Api.Responses
{
    public class ClientCredentialsGrantResponse : OsuApiResponse
    {
        public AccessToken Token { get; set; }
        public override async Task<OsuApiResponse?> Parse(HttpResponseMessage response)
        {
            string content = await response.Content.ReadAsStringAsync();

            JObject json = JObject.Parse(content);
            string? accessToken = (string?)json.GetValue("access_token");
            int? expiresInSeconds = (int?)json.GetValue("expires_in");
            string? tokenType = (string?)json.GetValue("token_type");

            if (accessToken == null || expiresInSeconds == null || tokenType == null)
                throw await Malformed(response);

            Token = new AccessToken(accessToken, (int)expiresInSeconds, tokenType);
            return this;
        }
    }
}
