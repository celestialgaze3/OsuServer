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

        /// <summary>
        /// Starts this client by requesting a client credentials grant.
        /// </summary>
        public async Task Start()
        {
            ClientCredentialsGrantRequest request = new(Id, Secret);
            ClientCredentialsGrantResponse response = await request.Send(_client);
            AccessToken = response.Token;
        }

        public async Task<TResponse> SendRequest<TResponse>(OsuApiRequest<TResponse> request) where TResponse : OsuApiResponse, new()
        {
            if (AccessToken == null)
                throw new InvalidOperationException("Access token must be granted with Start() before using this client");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AccessToken.Type, AccessToken.Value);

            return await request.Send(_client);
        }
    }
}
