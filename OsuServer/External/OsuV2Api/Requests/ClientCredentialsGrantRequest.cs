using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json.Linq;
using OsuServer.External.OsuV2Api.Responses;

namespace OsuServer.External.OsuV2Api.Requests
{
    public class ClientCredentialsGrantRequest : OsuApiRequest<ClientCredentialsGrantResponse>
    {
        public ClientCredentialsGrantRequest(int clientId, string clientSecret) :
            base(new Uri("https://osu.ppy.sh/oauth/token"),
                HttpMethod.Post,
                EncodeContent(
                    new KeyValuePair<string, string?>("client_id", Convert.ToString(clientId)),
                    new KeyValuePair<string, string?>("client_secret", clientSecret),
                    new KeyValuePair<string, string?>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string?>("scope", "public")
                )
            )
        { }
    }
}
