namespace OsuServer.External.OsuV2Api
{
    public class AccessToken
    {
        public string Value { get; set; }
        public bool Expired { 
            get
            {
                return (DateTime.Now - _expireTime).TotalSeconds >= 0;
            }
        }
        private DateTime _expireTime;
        public string Type { get; set; }

        public AccessToken(string accessToken, int expiresInSeconds, string tokenType) 
        { 
            Value = accessToken;
            _expireTime = DateTime.Now.AddSeconds(expiresInSeconds);
            Type = tokenType;
        }
    }
}
