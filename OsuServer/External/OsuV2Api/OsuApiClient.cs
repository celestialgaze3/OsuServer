using OsuServer.External.OsuV2Api.Requests;
using OsuServer.External.OsuV2Api.Responses;
using System.Net.Http.Headers;

namespace OsuServer.External.OsuV2Api
{
    public class OsuApiClient
    {
        protected HttpClient _client = new HttpClient();
        public int Id { get; set; }
        public string Secret { get; set; }
        public AccessToken? AccessToken { get; private set; }
        public OsuApiClient(int id, string secret) 
        {
            Id = id;
            Secret = secret;

            SetupClient();
        }

        private void SetupClient()
        {
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task RenewToken()
        {
            Console.WriteLine("Renewing osu!api token...");
            ClientCredentialsGrantRequest request = new(Id, Secret);
            ClientCredentialsGrantResponse? response = await request.Send(_client);
            if (response == null)
                throw new InvalidOperationException("Unable to access osu! API; client credentials grant returned null");
            AccessToken = response.Token;
        }

        /// <summary>
        /// Starts this client by requesting a client credentials grant.
        /// </summary>
        public async Task Start()
        {
            await RenewToken();
        }

        public async Task<TResponse?> SendRequest<TResponse>(OsuApiRequest<TResponse> request) where TResponse : OsuApiResponse, new()
        {
            // Ensure the client has been started before usage
            if (AccessToken == null)
                throw new InvalidOperationException("Access token must be granted with Start() before using this client");
            
            // Renew the access token if it has expired
            if (AccessToken.Expired)
                await RenewToken();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AccessToken.Type, AccessToken.Value);

            return await request.Send(_client);
        }
    }
}
