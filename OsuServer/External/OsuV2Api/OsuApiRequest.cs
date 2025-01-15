using Newtonsoft.Json.Linq;

namespace OsuServer.External.OsuV2Api
{
    public abstract class OsuApiRequest<TResponse> where TResponse : OsuApiResponse, new()
    {
        public Uri Uri { get; private set; }
        public HttpMethod Method { get; private set; }
        public HttpContent Content { get; private set; }
        public OsuApiRequest(Uri uri, HttpMethod method, HttpContent content) 
        {
            Uri = uri;
            Method = method;
            Content = content;
        }

        public virtual async Task<TResponse> Send(HttpClient client)
        {
            HttpRequestMessage request = new()
            {
                RequestUri = Uri,
                Method = Method
            };

            if (Method == HttpMethod.Get)
            {
                string requestParams = await Content.ReadAsStringAsync();
                request.RequestUri = new Uri($"{Uri.OriginalString}?{requestParams}");
                Console.WriteLine("Get request parameterization result:" + request.RequestUri.ToString());
            } 
            else if (Method == HttpMethod.Post)
            {
                request.Content = Content;
            }
            Console.WriteLine($"Sending request to the osu! API at {Uri.ToString()} ({Method.ToString()})");

            HttpResponseMessage response = await client.SendAsync(request);

            Console.WriteLine("Got response: " + await response.Content.ReadAsStringAsync());
            return (TResponse) await new TResponse().Parse(response); // surely this works
        }

        /// <summary>
        /// Encodes KeyValuePairs into a FormUrlEncodedContent object, ignoring null values
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <returns></returns>
        public static FormUrlEncodedContent EncodeContent(params KeyValuePair<string, string?>[] keyValuePairs)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

            foreach (var keyValuePair in keyValuePairs)
            {
                if (keyValuePair.Value != null)
                {
                    list.Add(new KeyValuePair<string, string>(keyValuePair.Key, keyValuePair.Value));
                }
            }

            FormUrlEncodedContent content = new FormUrlEncodedContent(list);

            return content;
        }

    }
}
