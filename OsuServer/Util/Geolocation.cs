using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace OsuServer.Util
{
    public class Geolocation
    {
        private float? _latitude = null;
        private float? _longitude = null;
        private CountryCode? _countryCode = null;
        public float Latitude
        {
            get
            {
                return _latitude ?? 0;
            }
            set
            {
                _latitude = value;
            }
        }

        public float Longitude
        {
            get
            {
                return _longitude ?? 0;
            }
            set
            {
                _longitude = value;
            }
        }

        public CountryCode CountryCode
        {
            get
            {
                return _countryCode ?? CountryCode.US;
            }
            set
            {
                _countryCode = value;
            }
        }

        public bool LatitudeIsNull => _latitude == null;
        public bool LongitudeIsNull => _longitude == null;
        public bool CountryCodeIsNull => _countryCode == null;

        public Geolocation() { }

        public Geolocation(float? latitude, float? longitude, CountryCode? countryCode)
        {
            _latitude = latitude;
            _longitude = longitude;
            _countryCode = countryCode;
        }

        public static async Task<Geolocation> Retrieve(HttpContext context, string? ip)
        {
            float? latitude = null;
            float? longitude = null;
            CountryCode? countryCode = null;

            // Cloudflare headers can give us geolocation data
            if (context.Request.Headers.TryGetValue("CF-IPLatitude", out StringValues latitudeHeader))
            {
                string? latitudeStr = latitudeHeader[0];
                if (latitudeStr != null)
                    latitude = float.Parse(latitudeStr);
            }

            if (context.Request.Headers.TryGetValue("CF-IPLongitude", out StringValues longitudeHeader))
            {
                string? longitudeStr = longitudeHeader[0];
                if (longitudeStr != null)
                    longitude = float.Parse(longitudeStr);
            }

            if (context.Request.Headers.TryGetValue("CF-IPCountry", out StringValues countryCodeHeader))
            {
                string? countryCodeStr = countryCodeHeader[0];
                if (countryCodeStr != null)
                {
                    countryCode = Enum.Parse<CountryCode>(countryCodeStr);
                }
            }

            // If information was not found in the headers, we need to query an external API
            bool informationIsComplete = latitude != null && longitude != null && countryCode != null;
            if (ip != null && !informationIsComplete)
            {
                Console.WriteLine("Geolocation information in headers not complete, querying external API...");

                // Make a request to ip-api.com
                try
                {
                    HttpClient client = new();
                    string jsonResponse =
                        await client.GetStringAsync($"http://ip-api.com/json/{ip}?fields=status,message,lat,lon,countryCode");
                    JObject json = JObject.Parse(jsonResponse);

                    // Response parameters
                    JToken? responseStatus = json["status"];
                    JToken? responseMessage = json["message"];
                    JToken? responseLatitude = json["lat"];
                    JToken? responseLongitude = json["lon"];
                    JToken? responseCountryCode = json["countryCode"];
                    string? responseCountryCodeStr = null;

                    // Handle response state
                    if (responseStatus != null)
                    {
                        string? statusStr = responseStatus.Value<string>();

                        // Successful responses
                        if (statusStr == "success")
                        {
                            if (responseLatitude != null)
                                latitude = responseLatitude.Value<float>();
                            if (responseLongitude != null)
                                longitude = responseLongitude.Value<float>();
                            if (responseCountryCode != null)
                                responseCountryCodeStr = responseCountryCode.Value<string>();
                            if (responseCountryCodeStr != null)
                                countryCode = Enum.Parse<CountryCode>(responseCountryCodeStr);
                        }
                        // Unsuccessful responses
                        else if (responseMessage != null)
                        {
                            string? message = responseMessage.Value<string>();
                            Console.WriteLine($"(Status {statusStr}) Unable to retrieve geolocation: {message}");
                        }
                    }
                }
                catch (Exception ex) 
                {
                    Console.WriteLine($"Unable to retrieve geolocation: {ex.Message}");
                }
            }

            Console.WriteLine($"Determined user's geolocation to be lat'{latitude}' lon'{longitude}' cc'{countryCode}'");
            return new Geolocation(latitude, longitude, countryCode);
        }
    }
}
