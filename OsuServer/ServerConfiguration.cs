namespace OsuServer
{
    public static class ServerConfiguration
    {
        private const string _configName = "config.json";
        private static IConfiguration Config { get; } = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Environment.CurrentDirectory, _configName),
                optional: false,
                reloadOnChange: true).Build();

        public static string Domain { 
            get
            {
                return Config.TryGetValue<string>("domain");
            }
        }
        public static string ClientSecret { 
            get
            {
                return Config.GetSection("osuApi").TryGetValue<string>("clientSecret");
            }
        }
        public static int ClientId { 
            get
            {
                return Config.GetSection("osuApi").TryGetValue<int>("clientId");
            }
        }

        private static T TryGetValue<T>(this IConfiguration config, string name)
        {
            bool exists = config.GetSection(name).Value != null;
            T? result = config.GetValue<T>(name);

            if (!exists || result == null)
            {
                throw new InvalidOperationException($"The field {name} is missing from {_configName}");
            }

            return result;
        }
    }
}
